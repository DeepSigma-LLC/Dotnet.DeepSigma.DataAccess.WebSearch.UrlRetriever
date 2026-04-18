namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Exceptions;

/// <summary>
/// Thrown when the SearXNG instance returns HTTP 403, indicating that the JSON response
/// format is disabled in the instance's configuration.
/// </summary>
/// <remarks>
/// JSON and other non-HTML formats can be toggled per-instance via the SearXNG
/// <c>search.formats</c> setting. This exception signals that the targeted instance
/// does not permit programmatic JSON access. Consider switching to a self-hosted
/// or otherwise controlled instance to ensure JSON support is available.
/// </remarks>
public sealed class SearxngUnsupportedFormatException : SearxngException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">A message describing the unsupported format condition.</param>
    public SearxngUnsupportedFormatException(string message) 
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message describing the unsupported format condition.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public SearxngUnsupportedFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}
