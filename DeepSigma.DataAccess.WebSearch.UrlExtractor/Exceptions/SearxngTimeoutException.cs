namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Exceptions;

/// <summary>
/// Thrown when a search request to SearXNG exceeds the configured timeout.
/// </summary>
public sealed class SearxngTimeoutException : SearxngException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">A message that describes the timeout condition.</param>
    public SearxngTimeoutException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message that describes the timeout condition.</param>
    /// <param name="innerException">
    /// The underlying <see cref="System.Threading.Tasks.TaskCanceledException"/> that triggered the timeout.
    /// </param>
    public SearxngTimeoutException(string message, Exception innerException)
        : base(message, innerException) { }
}
