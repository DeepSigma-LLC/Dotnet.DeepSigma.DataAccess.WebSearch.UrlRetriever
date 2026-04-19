
namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

/// <summary>
/// Represents the optional parameters that can be included in a search request to a SearXNG instance.
/// </summary>
/// <param name="Page">
/// One-based page number for paginating through results.
/// <see langword="null"/> requests the first page (the SearXNG default).
/// </param>
/// <param name="Language">
/// BCP-47 language/region code to constrain results, e.g. <c>en</c> or <c>en-US</c>.
/// <see langword="null"/> defers to the instance default.
/// </param>
/// <param name="TimeRange">
/// Time-range filter token accepted by SearXNG.
/// Common values are <c>day</c>, <c>week</c>, <c>month</c>, and <c>year</c>.
/// <see langword="null"/> applies no time filter.
/// </param>
/// <param name="SafeSearch">
/// Safe-search filtering level. <see langword="null"/> defers to the instance default.
/// </param>
/// <param name="Categories">
/// One or more SearXNG category names that limit the search scope,
/// e.g. <c>general</c>, <c>images</c>, <c>news</c>.
/// <see langword="null"/> defers to the instance default.
/// </param>
/// <param name="Engines">
/// One or more engine names that restrict the search to specific providers,
/// e.g. <c>google</c>, <c>bing</c>.
/// <see langword="null"/> defers to the instance default.
/// </param>
public sealed record SearchRequestOptions(
    int? Page = null,
    string? Language = null,
    string? TimeRange = null,
    SafeSearchLevel? SafeSearch = null,
    IReadOnlyList<string>? Categories = null,
    IReadOnlyList<string>? Engines = null);
