namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

/// <summary>
/// Controls the safe-search filtering level sent to SearXNG via the <c>safesearch</c> query parameter.
/// </summary>
public enum SafeSearchLevel
{
    /// <summary>
    /// Safe-search filtering is disabled. Corresponds to <c>safesearch=0</c>.
    /// </summary>
    Off = 0,

    /// <summary>
    /// Moderate safe-search filtering is applied. Corresponds to <c>safesearch=1</c>.
    /// </summary>
    Moderate = 1,

    /// <summary>
    /// Strict safe-search filtering is applied. Corresponds to <c>safesearch=2</c>.
    /// </summary>
    Strict = 2
}
