namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Exceptions;

/// <summary>
/// Abstract base class for all exceptions thrown by the SearXNG client library.
/// Catch this type to handle any SearXNG-specific failure in a single handler.
/// </summary>
public abstract class SearxngException : Exception
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    protected SearxngException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    protected SearxngException(string message, Exception innerException)
        : base(message, innerException) { }
}
