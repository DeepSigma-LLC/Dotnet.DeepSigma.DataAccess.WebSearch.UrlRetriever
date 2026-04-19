using DeepSigma.DataAccess.WebSearch.UrlRetriever.Internal;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Xunit;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever.Test;

public class QueryBuilderTests
{
    [Fact]
    public void Build_SimpleQuery_ContainsEncodedQueryAndFormat()
    {
        var result = SearxngQueryBuilder.Build("test query", new SearchRequestOptions());

        Assert.Contains("q=test%20query", result);
        Assert.Contains("format=json", result);
    }

    [Fact]
    public void Build_WithPage_ContainsPageno()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(Page: 3));

        Assert.Contains("pageno=3", result);
    }

    [Fact]
    public void Build_WithLanguage_ContainsLanguage()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(Language: "en-US"));

        Assert.Contains("language=en-US", result);
    }

    [Fact]
    public void Build_WithTimeRange_ContainsTimeRange()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(TimeRange: "week"));

        Assert.Contains("time_range=week", result);
    }

    [Fact]
    public void Build_WithSafeSearchStrict_ContainsSafesearch2()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(SafeSearch: SafeSearchLevel.Strict));

        Assert.Contains("safesearch=2", result);
    }

    [Fact]
    public void Build_WithCategories_ContainsDecodedCategoryList()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(Categories: ["general", "news"]));
        var decoded = Uri.UnescapeDataString(result);

        Assert.Contains("categories=general,news", decoded);
    }

    [Fact]
    public void Build_WithEngines_ContainsDecodedEngineList()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions(Engines: ["google", "bing"]));
        var decoded = Uri.UnescapeDataString(result);

        Assert.Contains("engines=google,bing", decoded);
    }

    [Fact]
    public void Build_WithoutOptionalParams_OmitsOptionalKeys()
    {
        var result = SearxngQueryBuilder.Build("test", new SearchRequestOptions());

        Assert.DoesNotContain("pageno", result);
        Assert.DoesNotContain("language", result);
        Assert.DoesNotContain("time_range", result);
        Assert.DoesNotContain("safesearch", result);
        Assert.DoesNotContain("categories", result);
        Assert.DoesNotContain("engines", result);
    }
}
