namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

/// <summary>
/// Represents a non-fatal warning attached to a <see cref="SearchResponse"/>, such as a disabled engine or a degraded upstream provider.
/// </summary>
/// <param name="Message">A human-readable description of the warning condition.</param>
/// <param name="Engine">
/// The name of the engine that triggered this warning, if applicable.
/// </param>
/// <param name="ErrorCode">
/// The error type reported by SearXNG for the unresponsive engine, if applicable.
/// Common values include <c>HTTP error</c>, <c>timeout</c>, and <c>no results</c>.
/// </param>
public sealed record SearchWarning(string Message, string? Engine = null, string? ErrorCode = null);
