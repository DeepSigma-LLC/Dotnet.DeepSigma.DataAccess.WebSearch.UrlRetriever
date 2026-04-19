using System.Net;
using System.Text;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Exceptions;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Test;

public class SearxngClientTests
{
    private static readonly Uri BaseUri = new("http://test.local");

    private static readonly SearxngOptions DefaultOptions = new()
    {
        BaseUri = BaseUri,
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static SearxngClient CreateClient(HttpResponseMessage response, SearxngOptions? options = null)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(response))
        {
            BaseAddress = BaseUri
        };
        return new SearxngClient(httpClient, Options.Create(options ?? DefaultOptions), NullLogger<SearxngClient>.Instance);
    }

    private static SearxngClient CreateClientWithException(Exception exception)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(exception))
        {
            BaseAddress = BaseUri
        };
        return new SearxngClient(httpClient, Options.Create(DefaultOptions), NullLogger<SearxngClient>.Instance);
    }

    [Fact]
    public async Task SearchAsync_ValidResponse_ReturnsResults()
    {
        const string json = """
            {
                "results": [
                    { "title": "Example", "url": "https://example.com", "content": "A snippet", "engine": "google" }
                ],
                "number_of_results": 1
            }
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await client.SearchAsync("test", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(result.Results);
        Assert.Equal("Example", result.Results[0].Title);
        Assert.Equal("https://example.com", result.Results[0].Url);
        Assert.Equal("A snippet", result.Results[0].Snippet);
        Assert.Equal("google", result.Results[0].Engine);
    }

    [Fact]
    public async Task SearchAsync_EmptyResults_ReturnsEmptyList()
    {
        const string json = """{"results":[],"number_of_results":0}""";

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await client.SearchAsync("test", cancellationToken: CancellationToken.None);

        Assert.Empty(result.Results);
        Assert.Equal(0, result.Metadata.ResultCount);
    }

    [Fact]
    public async Task SearchAsync_NullRequest_ThrowsArgumentNullException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.SearchAsync(null!, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SearchAsync(string.Empty, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_WhitespaceQuery_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SearchAsync("   ", cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_ForbiddenResponse_ThrowsUnsupportedFormatException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Forbidden));

        await Assert.ThrowsAsync<SearxngUnsupportedFormatException>(() =>
            client.SearchAsync("test", cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_BadRequest_ThrowsBadRequestException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var ex = await Assert.ThrowsAsync<SearxngBadRequestException>(() =>
            client.SearchAsync("test", cancellationToken: CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task SearchAsync_NetworkFailure_ThrowsUnavailableException()
    {
        var client = CreateClientWithException(new HttpRequestException("connection refused"));

        await Assert.ThrowsAsync<SearxngUnavailableException>(() =>
            client.SearchAsync("test", cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_MalformedJson_ThrowsParseException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        });

        await Assert.ThrowsAsync<SearxngParseException>(() =>
            client.SearchAsync("test", cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_MetadataReflectsQueryAndPage()
    {
        const string json = """{"results":[],"number_of_results":0}""";

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await client.SearchAsync("hello", new SearchRequestOptions(Page: 2), CancellationToken.None);

        Assert.Equal("hello", result.Metadata.Query);
        Assert.Equal(2, result.Metadata.Page);
        Assert.Equal(BaseUri.ToString(), result.Metadata.InstanceBaseUrl);
    }
}
