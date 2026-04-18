namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Exceptions;

/// <summary>
/// Thrown when the SearXNG response body cannot be deserialized into the expected JSON shape.
/// </summary>
/// <remarks>
/// This exception wraps a <see cref="System.Text.Json.JsonException"/> when the body is
/// structurally invalid, or is thrown directly when the deserialized result is <see langword="null"/>
/// (e.g. an empty response body).
/// </remarks>
public sealed class SearxngParseException : SearxngException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">A message that describes the parse failure.</param>
    public SearxngParseException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="message">A message that describes the parse failure.</param>
    /// <param name="innerException">
    /// The underlying <see cref="System.Text.Json.JsonException"/> that caused the failure.
    /// </param>
    public SearxngParseException(string message, Exception innerException)
        : base(message, innerException) { }
}
