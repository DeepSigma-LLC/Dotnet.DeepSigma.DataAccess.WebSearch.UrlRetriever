using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal.Dto;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Xunit;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Test;

public class ResponseMappingTests
{
    private static readonly Uri BaseUri = new("http://test.local/");

    [Fact]
    public void Map_WithValidResults_ReturnsCorrectResults()
    {
        var dto = new SearxngJsonResponse
        {
            Results =
            [
                new SearxngJsonResult { Title = "Result 1", Url = "https://example.com/1", Content = "Snippet", Engine = "google" },
                new SearxngJsonResult { Title = "Result 2", Url = "https://example.com/2", Content = null, Engine = "bing" }
            ]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.FromMilliseconds(50));

        Assert.Equal(2, response.Results.Count);
        Assert.Equal("Result 1", response.Results[0].Title);
        Assert.Equal("https://example.com/1", response.Results[0].Url);
        Assert.Equal("Snippet", response.Results[0].Snippet);
        Assert.Equal("google", response.Results[0].Engine);
        Assert.Null(response.Results[1].Snippet);
    }

    [Fact]
    public void Map_WithNullResults_ReturnsEmptyList()
    {
        var dto = new SearxngJsonResponse { Results = null };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.Empty(response.Results);
    }

    [Fact]
    public void Map_FiltersMissingAndBlankUrls()
    {
        var dto = new SearxngJsonResponse
        {
            Results =
            [
                new SearxngJsonResult { Title = "Valid", Url = "https://example.com" },
                new SearxngJsonResult { Title = "No URL", Url = null },
                new SearxngJsonResult { Title = "Blank URL", Url = "   " }
            ]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.Single(response.Results);
        Assert.Equal("Valid", response.Results[0].Title);
    }

    [Fact]
    public void Map_PopulatesMetadataCorrectly()
    {
        var dto = new SearxngJsonResponse { Results = [] };
        var duration = TimeSpan.FromMilliseconds(123);

        var response = SearxngResponseMapper.Map(dto, "my query", new SearchRequestOptions(Page: 2), BaseUri, duration);

        Assert.Equal("my query", response.Metadata.Query);
        Assert.Equal(2, response.Metadata.Page);
        Assert.Equal(duration, response.Metadata.Duration);
        Assert.Equal(BaseUri.ToString(), response.Metadata.InstanceBaseUrl);
        Assert.Equal(0, response.Metadata.ResultCount);
    }

    [Fact]
    public void Map_UsesEmptyStringForMissingTitle()
    {
        var dto = new SearxngJsonResponse
        {
            Results = [new SearxngJsonResult { Title = null, Url = "https://example.com" }]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.Equal(string.Empty, response.Results[0].Title);
    }

    [Fact]
    public void Map_MapsTotalResultsToMetadata()
    {
        var dto = new SearxngJsonResponse { Results = [], NumberOfResults = 1234 };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.Equal(1234, response.Metadata.TotalResults);
    }

    [Fact]
    public void Map_MapsAnswersCorrectionsAndSuggestions()
    {
        var dto = new SearxngJsonResponse
        {
            Results = [],
            Answers = ["42"],
            Corrections = ["did you mean: hello"],
            Suggestions = ["hello world", "hello there"]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.Equal(["42"], response.Answers);
        Assert.Equal(["did you mean: hello"], response.Corrections);
        Assert.Equal(["hello world", "hello there"], response.Suggestions);
    }

    [Fact]
    public void Map_NullAnswersCorrectionsAndSuggestions_ReturnsEmptyLists()
    {
        var dto = new SearxngJsonResponse { Results = [] };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);
        Assert.Empty(response.Answers);
        Assert.Empty(response.Corrections);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public void Map_UnresponsiveEngines_SetsPartialAndPopulatesWarnings()
    {
        var dto = new SearxngJsonResponse
        {
            Results = [],
            UnresponsiveEngines = [["google", "HTTP error"], ["bing", "timeout"]]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.True(response.Metadata.Partial);
        Assert.Equal(2, response.Warnings.Count);
        Assert.Equal("google", response.Warnings[0].Engine);
        Assert.Equal("HTTP error", response.Warnings[0].ErrorCode);
        Assert.Equal("google: HTTP error", response.Warnings[0].Message);
        Assert.Equal("bing", response.Warnings[1].Engine);
        Assert.Equal("timeout", response.Warnings[1].ErrorCode);
    }

    [Fact]
    public void Map_NoUnresponsiveEngines_PartialIsFalseAndWarningsEmpty()
    {
        var dto = new SearxngJsonResponse { Results = [] };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);

        Assert.False(response.Metadata.Partial);
        Assert.Empty(response.Warnings);
    }

    [Fact]
    public void Map_MapsResultMediaFields()
    {
        var dto = new SearxngJsonResponse
        {
            Results =
            [
                new SearxngJsonResult
                {
                    Title = "Media Result",
                    Url = "https://example.com",
                    Template = "images.html",
                    Thumbnail = "https://thumb.example.com/img.jpg",
                    ImageSrc = "https://full.example.com/img.jpg",
                    Author = "Jane Doe",
                    IframeSrc = "https://video.example.com/embed/1"
                }
            ]
        };

        var response = SearxngResponseMapper.Map(dto, "query", new SearchRequestOptions(), BaseUri, TimeSpan.Zero);
        var result = response.Results[0];

        Assert.Equal("images.html", result.Template);
        Assert.Equal("https://thumb.example.com/img.jpg", result.Thumbnail);
        Assert.Equal("https://full.example.com/img.jpg", result.ImageUrl);
        Assert.Equal("Jane Doe", result.Author);
        Assert.Equal("https://video.example.com/embed/1", result.IframeSrc);
    }
}
