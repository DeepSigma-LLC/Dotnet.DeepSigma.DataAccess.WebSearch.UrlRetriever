namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Exceptions;

/// <summary>
/// Thrown when the SearXNG instance cannot be reached due to a network-level failure,
/// such as a DNS resolution error, a refused connection, or an unreachable host.
/// </summary>
public sealed class SearxngUnavailableException : SearxngException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">A message that describes the connectivity failure.</param>
    public SearxngUnavailableException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message that describes the connectivity failure.</param>
    /// <param name="innerException">
    /// The underlying <see cref="System.Net.Http.HttpRequestException"/> or similar network error.
    /// </param>
    public SearxngUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
}
