namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Exceptions;

/// <summary>
/// Thrown when the SearXNG instance returns a 4xx HTTP response other than 403.
/// Inspect <see cref="StatusCode"/> to determine the exact HTTP status code.
/// </summary>
public sealed class SearxngBadRequestException : SearxngException
{
    /// <summary>
    /// Gets the HTTP status code returned by the SearXNG instance.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance with the specified error message and HTTP status code.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code returned by the SearXNG instance.</param>
    public SearxngBadRequestException(string message, int statusCode) 
        : base(message) => StatusCode = statusCode;

    /// <summary>
    /// Initializes a new instance with the specified error message, HTTP status code, and
    /// a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code returned by the SearXNG instance.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public SearxngBadRequestException(string message, int statusCode, Exception innerException)
        : base(message, innerException) => StatusCode = statusCode;
}
