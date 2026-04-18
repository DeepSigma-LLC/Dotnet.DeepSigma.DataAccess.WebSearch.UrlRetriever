namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

/// <summary>
/// Provides metadata about a completed search operation.
/// </summary>
/// <param name="InstanceBaseUrl">
/// The base URL of the SearXNG instance that handled the request,
/// as reported by <see cref="System.Net.Http.HttpClient.BaseAddress"/>.
/// </param>
/// <param name="Query">The original query string that was submitted.</param>
/// <param name="Page">
/// The page number that was requested, or <see langword="null"/> if the default first page was used.
/// </param>
/// <param name="Duration">The total elapsed time from request dispatch to response mapping.</param>
/// <param name="Partial">
/// <see langword="true"/> when the response represents a partial result set, for example because
/// one or more upstream engines were disabled or returned errors; otherwise <see langword="false"/>.
/// </param>
/// <param name="ResultCount">
/// The number of results present in the associated <see cref="SearchResponse.Results"/> collection.
/// </param>
/// <param name="TotalResults">
/// The total number of results reported by SearXNG across all pages, if provided.
/// This value typically far exceeds <see cref="ResultCount"/>, which reflects only the current page.
/// </param>
public sealed record SearchMetadata(
    string InstanceBaseUrl,
    string Query,
    int? Page,
    TimeSpan Duration,
    bool Partial,
    int ResultCount,
    long? TotalResults = null);
