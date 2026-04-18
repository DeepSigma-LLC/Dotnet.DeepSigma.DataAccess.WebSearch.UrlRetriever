using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Xunit;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Test;

public class QueryBuilderTests
{
    [Fact]
    public void Build_SimpleQuery_ContainsEncodedQueryAndFormat()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test query"));

        Assert.Contains("q=test%20query", result);
        Assert.Contains("format=json", result);
    }

    [Fact]
    public void Build_WithPage_ContainsPageno()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", Page: 3));

        Assert.Contains("pageno=3", result);
    }

    [Fact]
    public void Build_WithLanguage_ContainsLanguage()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", Language: "en-US"));

        Assert.Contains("language=en-US", result);
    }

    [Fact]
    public void Build_WithTimeRange_ContainsTimeRange()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", TimeRange: "week"));

        Assert.Contains("time_range=week", result);
    }

    [Fact]
    public void Build_WithSafeSearchStrict_ContainsSafesearch2()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", SafeSearch: SafeSearchLevel.Strict));

        Assert.Contains("safesearch=2", result);
    }

    [Fact]
    public void Build_WithCategories_ContainsDecodedCategoryList()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", Categories: ["general", "news"]));
        var decoded = Uri.UnescapeDataString(result);

        Assert.Contains("categories=general,news", decoded);
    }

    [Fact]
    public void Build_WithEngines_ContainsDecodedEngineList()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test", Engines: ["google", "bing"]));
        var decoded = Uri.UnescapeDataString(result);

        Assert.Contains("engines=google,bing", decoded);
    }

    [Fact]
    public void Build_WithoutOptionalParams_OmitsOptionalKeys()
    {
        var result = SearxngQueryBuilder.Build(new SearchRequest("test"));

        Assert.DoesNotContain("pageno", result);
        Assert.DoesNotContain("language", result);
        Assert.DoesNotContain("time_range", result);
        Assert.DoesNotContain("safesearch", result);
        Assert.DoesNotContain("categories", result);
        Assert.DoesNotContain("engines", result);
    }
}
