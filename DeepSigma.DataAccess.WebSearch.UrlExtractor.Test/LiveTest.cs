using DeepSigma.DataAccess.WebSearch.UrlExtractor.Exceptions;
using DeepSigma.DataAccess.WebSearch.UrlExtractor.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DeepSigma.DataAccess.WebSearch.UrlExtractor.Test;

public class LiveTest
{
    private static readonly Uri InstanceUri = new("http://localhost:8080");

    /// <summary>
    /// Probes <see cref="InstanceUri"/> with a short timeout. Returns <see langword="true"/>
    /// when the host is reachable (any HTTP response is sufficient), <see langword="false"/>
    /// on any network-level failure.
    /// </summary>
    private static async Task<bool> IsInstanceReachableAsync()
    {
        using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            await probe.GetAsync(InstanceUri);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task LiveTest_SearxngRequired()
    {
        if (!await IsInstanceReachableAsync())
            Assert.Skip($"No SearXNG instance reachable at {InstanceUri}. Start one to run live tests.");

        var services = new ServiceCollection();

        services.AddSearxngClient(new SearxngOptions
        {
            BaseUri   = InstanceUri,
            Timeout   = TimeSpan.FromSeconds(10),
            UserAgent = "MyApp/1.0"
        });

        await using var provider = services.BuildServiceProvider();

        ISearxngClient searxng = provider.GetRequiredService<ISearxngClient>();
        using CancellationTokenSource cts = new();
        CancellationToken ct = cts.Token;

        try
        {
            var response = await searxng.SearchAsync(new SearchRequest("open source"), ct);

            Assert.NotEmpty(response.Results);
        }
        catch (SearxngUnsupportedFormatException)
        {
            Assert.Skip($"JSON format is disabled on the SearXNG instance at {InstanceUri}. " +
                        "Add 'json' to search.formats in settings.yml and restart the instance.");
        }
        catch (SearxngUnavailableException ex)
        {
            Assert.Fail($"SearXNG instance became unreachable during the request: {ex.Message}" +
                        (ex.InnerException is not null ? $": {ex.InnerException.Message}" : ""));
        }
        catch (SearxngException ex)
        {
            Assert.Fail($"Search failed: {ex.Message}" +
                        (ex.InnerException is not null ? $": {ex.InnerException.Message}" : ""));
        }
    }
}
