using DeepSigma.DataAccess.WebSearch.UrlExtractor.Models;

namespace DeepSigma.DataAccess.WebSearch.UrlExtractor;

/// <summary>
/// Defines the contract for executing searches against a SearXNG instance.
/// </summary>
/// <remarks>
/// Implementations are expected to be registered as a typed <see cref="HttpClient"/>
/// via <see cref="ServiceCollectionExtensions.AddSearxngClient"/>. The interface is intentionally
/// provider-neutral so that alternative search backends can be swapped in behind the same abstraction.
/// </remarks>
public interface ISearxngClient
{
    /// <summary>
    /// Executes a search against the configured SearXNG instance and returns the normalized results.
    /// </summary>
    /// <param name="request">The search parameters to send to the SearXNG instance.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="SearchResponse"/> containing the list of results, response metadata,
    /// and any non-fatal warnings produced during the search.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="SearchRequest.Query"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="Exceptions.SearxngUnavailableException">
    /// Thrown when the SearXNG instance cannot be reached due to a network-level failure.
    /// </exception>
    /// <exception cref="Exceptions.SearxngTimeoutException">
    /// Thrown when the request exceeds the configured timeout.
    /// </exception>
    /// <exception cref="Exceptions.SearxngBadRequestException">
    /// Thrown when the instance returns a 4xx HTTP response other than 403.
    /// </exception>
    /// <exception cref="Exceptions.SearxngUnsupportedFormatException">
    /// Thrown when the instance returns HTTP 403, indicating that the JSON format is disabled.
    /// </exception>
    /// <exception cref="Exceptions.SearxngParseException">
    /// Thrown when the response body cannot be deserialized into the expected shape.
    /// </exception>
    Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
}
