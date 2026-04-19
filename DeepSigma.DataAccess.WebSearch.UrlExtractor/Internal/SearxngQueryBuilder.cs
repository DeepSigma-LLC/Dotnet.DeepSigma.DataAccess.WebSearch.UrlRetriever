using System.Globalization;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;

/// <summary>
/// Builds URL-encoded query strings from a query and <see cref="SearchRequestOptions"/>.
/// </summary>
/// <remarks>
/// Centralizing parameter encoding here ensures that all SearXNG-specific parameter names
/// and encoding rules are defined in one place, making the mapping easy to unit-test
/// independently of the HTTP layer.
/// </remarks>
internal static class SearxngQueryBuilder
{
    /// <summary>
    /// Converts a query and <see cref="SearchRequestOptions"/> into a percent-encoded query string
    /// suitable for appending to the SearXNG search path.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="requestOptions">
    /// </param>
    /// <returns>
    /// A percent-encoded query string, e.g.
    /// <c>q=hello%20world&amp;format=json&amp;pageno=2&amp;language=en</c>.
    /// The <c>format</c> parameter is always set to <c>json</c>.
    /// Optional parameters are omitted when their corresponding request properties are
    /// <see langword="null"/>, empty, or whitespace.
    /// </returns>
    internal static string Build(string query, SearchRequestOptions requestOptions)
    {
        List<string> parts = [
            $"q={Uri.EscapeDataString(query)}", 
            "format=json"];

        if (requestOptions.Page is int p)
            parts.Add($"pageno={p.ToString(CultureInfo.InvariantCulture)}");

        if (!string.IsNullOrWhiteSpace(requestOptions.Language))
            parts.Add($"language={Uri.EscapeDataString(requestOptions.Language)}");

        if (!string.IsNullOrWhiteSpace(requestOptions.TimeRange))
            parts.Add($"time_range={Uri.EscapeDataString(requestOptions.TimeRange)}");

        if (requestOptions.SafeSearch is not null)
            parts.Add($"safesearch={((int)requestOptions.SafeSearch.Value).ToString(CultureInfo.InvariantCulture)}");

        if (requestOptions.Categories?.Count > 0)
            parts.Add($"categories={Uri.EscapeDataString(string.Join(",", requestOptions.Categories))}");

        if (requestOptions.Engines?.Count > 0)
            parts.Add($"engines={Uri.EscapeDataString(string.Join(",", requestOptions.Engines))}");

        return string.Join("&", parts);
    }
}
