namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

/// <summary>
/// Represents a single normalized search result returned by a SearXNG query.
/// </summary>
/// <param name="Title">The display title of the result page.</param>
/// <param name="Url">The canonical URL of the result page.</param>
/// <param name="Snippet">
/// A short excerpt or description from the result page content, if available.
/// Mapped from the SearXNG <c>content</c> field.
/// </param>
/// <param name="Engine">
/// The name of the search engine that produced this result, if reported by SearXNG.
/// </param>
/// <param name="ParsedUrls">
/// Additional URLs extracted from the result content, if available.
/// </param>
/// <param name="Engines">
/// All search engines that contributed this result when SearXNG de-duplicates across providers.
/// </param>
/// <param name="Score">
/// The aggregated relevance score assigned by SearXNG. Higher values indicate higher relevance.
/// </param>
/// <param name="Category">
/// The SearXNG category this result belongs to, e.g. <c>general</c> or <c>news</c>.
/// </param>
/// <param name="PublishedDate">
/// The publication or last-modified date of the result page, if reported by the engine.
/// </param>
/// <param name="PrettyUrl">
/// A human-readable, shortened form of the URL suitable for display, if provided by SearXNG.
/// </param>
/// <param name="Template">
/// The result template type reported by SearXNG.
/// Common values: <c>default.html</c>, <c>images.html</c>, <c>videos.html</c>,
/// <c>torrent.html</c>, <c>map.html</c>, <c>code.html</c>.
/// </param>
/// <param name="Thumbnail">
/// URL of a thumbnail image associated with the result.
/// Typically present on image, video, and news results.
/// </param>
/// <param name="ImageUrl">
/// URL of the full-size image for image search results.
/// </param>
/// <param name="Author">
/// Author or byline of the result page, if reported by the engine.
/// Typically present on news and article results.
/// </param>
/// <param name="IframeSrc">
/// URL of an embeddable iframe, present on video results.
/// </param>
public sealed record SearchResult(
    string Title,
    string Url,
    string? Snippet,
    string? Engine,
    IReadOnlyList<string>? ParsedUrls = null,
    IReadOnlyList<string>? Engines = null,
    double? Score = null,
    string? Category = null,
    DateTimeOffset? PublishedDate = null,
    string? PrettyUrl = null,
    string? Template = null,
    string? Thumbnail = null,
    string? ImageUrl = null,
    string? Author = null,
    string? IframeSrc = null);
