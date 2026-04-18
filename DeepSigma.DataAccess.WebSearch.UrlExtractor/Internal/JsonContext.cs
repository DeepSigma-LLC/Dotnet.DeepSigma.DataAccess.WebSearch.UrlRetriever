using System.Text.Json.Serialization;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal.Dto;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> used to
/// deserialize <see cref="SearxngJsonResponse"/> without runtime reflection.
/// </summary>
/// <remarks>
/// Using source generation makes deserialization trim-safe and compatible with
/// ahead-of-time (AOT) compilation. The context is accessed via <see cref="Default"/>
/// inside <see cref="SearxngClient"/>.
/// </remarks>
[JsonSerializable(typeof(SearxngJsonResponse))]
internal partial class JsonContext : JsonSerializerContext { }
