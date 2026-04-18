using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal.Dto;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;

/// <summary>
/// Maps an internal <see cref="SearxngJsonResponse"/> DTO to the public
/// <see cref="SearchResponse"/> domain model.
/// </summary>
/// <remarks>
/// Keeping this mapping logic in a dedicated static class makes it independently testable
/// without requiring an HTTP layer or a live SearXNG instance.
/// </remarks>
internal static class SearxngResponseMapper
{
    /// <summary>
    /// Converts a deserialized SearXNG JSON response into a <see cref="SearchResponse"/>.
    /// </summary>
    /// <param name="dto">The deserialized JSON response from the SearXNG instance.</param>
    /// <param name="request">
    /// The original search request, used to populate <see cref="SearchMetadata.Query"/>
    /// and <see cref="SearchMetadata.Page"/>.
    /// </param>
    /// <param name="baseUri">
    /// The base URI of the SearXNG instance, recorded in <see cref="SearchMetadata.InstanceBaseUrl"/>.
    /// </param>
    /// <param name="duration">
    /// The elapsed time of the full HTTP round trip, recorded in <see cref="SearchMetadata.Duration"/>.
    /// </param>
    /// <returns>
    /// A fully populated <see cref="SearchResponse"/> with mapped results and metadata.
    /// Results with a <see langword="null"/> or whitespace <c>url</c> field are excluded.
    /// </returns>
    internal static SearchResponse Map(
        SearxngJsonResponse dto,
        SearchRequest request,
        Uri baseUri,
        TimeSpan duration)
    {
        var results = dto.Results?
            .Where(r => !string.IsNullOrWhiteSpace(r.Url))
            .Select(r => new SearchResult(
                Title: r.Title ?? string.Empty,
                Url: r.Url!,
                Snippet: r.Content,
                Engine: r.Engine,
                Engines: r.Engines?.AsReadOnly(),
                Score: r.Score,
                Category: r.Category,
                PublishedDate: TryParseDate(r.PublishedDate),
                PrettyUrl: r.PrettyUrl,
                Template: r.Template,
                Thumbnail: r.Thumbnail,
                ImageUrl: r.ImageSrc,
                Author: r.Author,
                IframeSrc: r.IframeSrc))
            .ToList() ?? [];

        var warnings = dto.UnresponsiveEngines?
            .Where(e => e is { Count: >= 1 })
            .Select(e => new SearchWarning(
                Message: e.Count >= 2 ? $"{e[0]}: {e[1]}" : e[0],
                Engine: e[0],
                ErrorCode: e.Count >= 2 ? e[1] : null))
            .ToList() ?? [];

        return new SearchResponse(
            results,
            new SearchMetadata(
                baseUri.ToString(),
                request.Query,
                request.Page,
                duration,
                Partial: warnings.Count > 0,
                ResultCount: results.Count,
                TotalResults: dto.NumberOfResults),
            warnings,
            Answers: ToReadOnly(dto.Answers),
            Corrections: ToReadOnly(dto.Corrections),
            Suggestions: ToReadOnly(dto.Suggestions));
    }

    private static IReadOnlyList<string> ToReadOnly(List<string>? list) =>
        list is { Count: > 0 } ? list.AsReadOnly() : [];

    private static DateTimeOffset? TryParseDate(string? value) =>
        value is not null && DateTimeOffset.TryParse(value, out var dt) ? dt : null;
}
