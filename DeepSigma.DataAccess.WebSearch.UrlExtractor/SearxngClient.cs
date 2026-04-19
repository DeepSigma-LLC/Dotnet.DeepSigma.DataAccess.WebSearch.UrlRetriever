using System.Diagnostics;
using System.Net;
using System.Text.Json;
using DeepSigma.DataAccess.WebSearch.Abstraction;
using DeepSigma.DataAccess.WebSearch.Abstraction.Model;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Exceptions;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal.Dto;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever;

/// <summary>
/// SearXNG-backed implementation of <see cref="IUrlRetriever{TSearchOptions}"/> that retrieves
/// URLs from a SearXNG instance's JSON search endpoint.
/// </summary>
/// <remarks>
/// Register this client via <see cref="ServiceCollectionExtensions.AddSearxngClient(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{SearxngOptions})"/> rather than
/// constructing it directly. That extension method configures the <see cref="HttpClient"/>,
/// eagerly validates <see cref="SearxngOptions"/>, and attaches a standard resilience pipeline
/// (retry, circuit breaker, and attempt timeout).
/// </remarks>
public sealed class SearxngClient : IUrlRetriever<SearchRequestOptions>
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SearxngOptions> _options;
    private readonly ILogger<SearxngClient> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SearxngClient"/>.
    /// </summary>
    /// <param name="httpClient">
    /// The typed <see cref="HttpClient"/> configured and injected by
    /// <see cref="IHttpClientFactory"/>.
    /// </param>
    /// <param name="options">The resolved <see cref="SearxngOptions"/> for this client.</param>
    /// <param name="logger">Logger used to record query latency and result counts.</param>
    public SearxngClient(HttpClient httpClient, IOptions<SearxngOptions> options, ILogger<SearxngClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Executes a SearXNG search and returns the results mapped to the provider-neutral
    /// <see cref="ResponseUrlRetrival"/> shape defined by <see cref="IUrlRetriever{TSearchOptions}"/>.
    /// Delegates to <see cref="SearchRawAsync"/> and discards metadata/answers/corrections;
    /// use <see cref="SearchRawAsync"/> directly if those are needed.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="requestOptions">Optional parameters to customize the search request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="ResponseUrlRetrival"/> representing the search results.</returns>
    public async Task<List<ResponseUrlRetrival>> SearchAsync(string query, SearchRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        SearchResponse response = await SearchRawAsync(query, requestOptions, cancellationToken).ConfigureAwait(false);
        return MapToResponseUrlRetrival(response).ToList();
    }

    /// <summary>
    /// Executes a SearXNG search and returns the full <see cref="SearchResponse"/>, including
    /// result metadata, answers, corrections, suggestions, and unresponsive-engine warnings.
    /// </summary>
    /// <param name="query">The search query string. Cannot be null, empty, or whitespace.</param>
    /// <param name="requestOptions">Optional parameters to customize the search request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The raw <see cref="SearchResponse"/> returned by the SearXNG instance.</returns>
    /// <remarks>
    /// Builds a percent-encoded query string from <paramref name="requestOptions"/>, dispatches an
    /// HTTP GET to <see cref="SearxngOptions.SearchPath"/>, and maps the JSON response to a
    /// <see cref="SearchResponse"/>. HTTP status codes are translated to typed exceptions:
    /// <list type="table">
    ///   <listheader>
    ///     <term>Condition</term>
    ///     <description>Exception thrown</description>
    ///   </listheader>
    ///   <item>
    ///     <term>HTTP 403</term>
    ///     <description><see cref="SearxngUnsupportedFormatException"/> — JSON format is disabled on the instance.</description>
    ///   </item>
    ///   <item>
    ///     <term>HTTP 4xx (other)</term>
    ///     <description><see cref="SearxngBadRequestException"/> — includes the exact status code.</description>
    ///   </item>
    ///   <item>
    ///     <term>Request timeout</term>
    ///     <description><see cref="SearxngTimeoutException"/></description>
    ///   </item>
    ///   <item>
    ///     <term>Network failure</term>
    ///     <description><see cref="SearxngUnavailableException"/></description>
    ///   </item>
    ///   <item>
    ///     <term>Malformed JSON body</term>
    ///     <description><see cref="SearxngParseException"/></description>
    ///   </item>
    /// </list>
    /// </remarks>
    public async Task<SearchResponse> SearchRawAsync(string query, SearchRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var baseAddress = _httpClient.BaseAddress
            ?? throw new InvalidOperationException(
                "HttpClient.BaseAddress was not set; AddSearxngClient must run before use.");

        var sw = Stopwatch.StartNew();
        requestOptions ??= new SearchRequestOptions();
        var queryString = SearxngQueryBuilder.Build(query, requestOptions);
        var path = $"{_options.Value.SearchPath}?{queryString}";

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new SearxngUnsupportedFormatException(
                    "JSON format may be disabled on this SearXNG instance.");

            if ((int)response.StatusCode is >= 400 and < 500)
                throw new SearxngBadRequestException(
                    $"SearXNG returned {(int)response.StatusCode}.",
                    (int)response.StatusCode);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            SearxngJsonResponse? dto;
            try
            {
                dto = await JsonSerializer.DeserializeAsync(
                    stream,
                    JsonContext.Default.SearxngJsonResponse,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize SearXNG response for query {Query}", query);
                throw new SearxngParseException("Failed to deserialize the SearXNG response.", ex);
            }

            if (dto is null)
                throw new SearxngParseException("Response body was empty or could not be parsed.");

            var result = SearxngResponseMapper.Map(dto, query, requestOptions, baseAddress, sw.Elapsed);

            _logger.LogDebug(
                "SearXNG search completed in {ElapsedMs} ms with {Count} results",
                sw.ElapsedMilliseconds,
                result.Results.Count);

            return result;
        }
        catch (SearxngException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "SearXNG request timed out for query {Query}", query);
            throw new SearxngTimeoutException("The SearXNG request timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "SearXNG instance unreachable for query {Query}", query);
            throw new SearxngUnavailableException(
                "Failed to reach the SearXNG instance.", ex);
        }
        finally
        {
            sw.Stop();
        }
    }

    private static IEnumerable<ResponseUrlRetrival> MapToResponseUrlRetrival(SearchResponse response)
    {
        foreach (var item in response.Results)
        {
            ResponseUrlRetrival responseUrl = new(
                Url: item.Url,
                Title: item.Title,
                Snippet: item.Snippet,
                EngineRelevanceScore: item.Score,
                RetrievedAt: DateTimeOffset.UtcNow,
                ParsedUrls: item.ParsedUrls ?? [],
                Engines: item.Engines ?? [],
                Category: item.Category,
                SearchEngine: item.Engine,
                PublishedDate: item.PublishedDate,
                Author: item.Author,
                Thumbnail: item.Thumbnail,
                ImageUrl: item.ImageUrl,
                IframeSrc: item.IframeSrc,
                PrettyUrl: item.PrettyUrl
                );
            yield return responseUrl;
        }
    }

}
