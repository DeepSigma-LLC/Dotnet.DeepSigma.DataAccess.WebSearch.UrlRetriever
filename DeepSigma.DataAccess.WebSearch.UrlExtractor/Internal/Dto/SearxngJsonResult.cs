using System.Text.Json.Serialization;

namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Internal.Dto;

/// <summary>
/// Internal DTO that represents a single result object within the SearXNG JSON response array.
/// This type is mapped to the public <see cref="Models.SearchResult"/> by
/// <see cref="SearxngResponseMapper"/>.
/// </summary>
internal sealed class SearxngJsonResult
{
    /// <summary>The display title of the result page.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The canonical URL of the result page.
    /// Results with a <see langword="null"/> or whitespace URL are filtered out during mapping.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// A short excerpt from the result page content.
    /// Mapped to <see cref="Models.SearchResult.Snippet"/>.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>The name of the search engine that returned this result.</summary>
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    /// <summary>
    /// All search engines that returned this result when SearXNG de-duplicates across providers.
    /// </summary>
    [JsonPropertyName("engines")]
    public List<string>? Engines { get; set; }

    /// <summary>
    /// The aggregated relevance score assigned by SearXNG, based on engine positions.
    /// Higher values indicate higher relevance.
    /// </summary>
    [JsonPropertyName("score")]
    public double? Score { get; set; }

    /// <summary>The SearXNG category this result belongs to, e.g. <c>general</c> or <c>news</c>.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// The publication or last-modified date of the result page as reported by the engine,
    /// represented as a raw string. Parsed to a <see cref="DateTimeOffset"/> during mapping.
    /// </summary>
    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    /// <summary>A human-readable, shortened form of the URL suitable for display.</summary>
    [JsonPropertyName("pretty_url")]
    public string? PrettyUrl { get; set; }

    /// <summary>
    /// The result template type that SearXNG would use to render this result.
    /// Common values: <c>default.html</c>, <c>images.html</c>, <c>videos.html</c>,
    /// <c>torrent.html</c>, <c>map.html</c>, <c>code.html</c>.
    /// </summary>
    [JsonPropertyName("template")]
    public string? Template { get; set; }

    /// <summary>
    /// URL of a thumbnail image associated with the result.
    /// Present on image, video, and news results.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// URL of the full-size image for image search results.
    /// </summary>
    [JsonPropertyName("img_src")]
    public string? ImageSrc { get; set; }

    /// <summary>
    /// Author or byline of the result page, if reported by the engine.
    /// Typically present on news and article results.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// URL of an embeddable iframe for video results.
    /// </summary>
    [JsonPropertyName("iframe_src")]
    public string? IframeSrc { get; set; }
}
