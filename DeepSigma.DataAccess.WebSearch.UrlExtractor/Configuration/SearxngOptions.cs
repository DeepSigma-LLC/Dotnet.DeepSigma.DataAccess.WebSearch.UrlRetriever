using DeepSigma.DataAccess.WebSearch.Abstraction;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever;

/// <summary>
/// Strongly-typed configuration options for <see cref="IUrlRetriever{SearchRequestOptions}"/>.
/// </summary>
/// <remarks>
/// Register and validate these options by calling
/// <see cref="ServiceCollectionExtensions.AddSearxngClient(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{SearxngOptions})"/>. Options are validated
/// eagerly at application startup via <c>ValidateOnStart</c>. At minimum,
/// <see cref="BaseUri"/> must be set to an absolute URI before the client is used.
/// </remarks>
public sealed class SearxngOptions
{
    /// <summary>
    /// Absolute base URI of the SearXNG instance, e.g. https://searxng.example.com.
    /// Must be set to an absolute URI before the client is used; validated at startup.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Per-attempt timeout. Defaults to 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Path to the search endpoint. Defaults to /search.
    /// </summary>
    public string SearchPath { get; set; } = "/search";

    /// <summary>
    /// Optional User-Agent header value sent with every request.
    /// </summary>
    public string? UserAgent { get; set; }
}
