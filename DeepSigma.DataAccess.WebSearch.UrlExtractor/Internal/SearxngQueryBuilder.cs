using System.Globalization;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;

/// <summary>
/// Builds URL-encoded query strings from a <see cref="SearchRequest"/>.
/// </summary>
/// <remarks>
/// Centralizing parameter encoding here ensures that all SearXNG-specific parameter names
/// and encoding rules are defined in one place, making the mapping easy to unit-test
/// independently of the HTTP layer.
/// </remarks>
internal static class SearxngQueryBuilder
{
    /// <summary>
    /// Converts a <see cref="SearchRequest"/> into a percent-encoded query string
    /// suitable for appending to the SearXNG search path.
    /// </summary>
    /// <param name="request">
    /// The search request to encode. <see cref="SearchRequest.Query"/> must not be
    /// <see langword="null"/> — callers are responsible for validating this before calling.
    /// </param>
    /// <returns>
    /// A percent-encoded query string, e.g.
    /// <c>q=hello%20world&amp;format=json&amp;pageno=2&amp;language=en</c>.
    /// The <c>format</c> parameter is always set to <c>json</c>.
    /// Optional parameters are omitted when their corresponding request properties are
    /// <see langword="null"/>, empty, or whitespace.
    /// </returns>
    internal static string Build(SearchRequest request)
    {
        List<string> parts = [
            $"q={Uri.EscapeDataString(request.Query)}", 
            "format=json"];

        if (request.Page is int p)
            parts.Add($"pageno={p.ToString(CultureInfo.InvariantCulture)}");

        if (!string.IsNullOrWhiteSpace(request.Language))
            parts.Add($"language={Uri.EscapeDataString(request.Language)}");

        if (!string.IsNullOrWhiteSpace(request.TimeRange))
            parts.Add($"time_range={Uri.EscapeDataString(request.TimeRange)}");

        if (request.SafeSearch is not null)
            parts.Add($"safesearch={((int)request.SafeSearch.Value).ToString(CultureInfo.InvariantCulture)}");

        if (request.Categories?.Count > 0)
            parts.Add($"categories={Uri.EscapeDataString(string.Join(",", request.Categories))}");

        if (request.Engines?.Count > 0)
            parts.Add($"engines={Uri.EscapeDataString(string.Join(",", request.Engines))}");

        return string.Join("&", parts);
    }
}
