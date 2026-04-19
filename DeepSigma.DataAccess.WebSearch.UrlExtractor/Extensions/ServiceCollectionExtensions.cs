using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Resilience;
using DeepSigma.DataAccess.WebSearch.Abstraction;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

namespace DeepSigma.DataAccess.WebSearch.UrlRetriever;

/// <summary>
/// Extension methods for registering SearXNG client services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IUrlRetriever{TSearchOptions}"/> with a typed <see cref="HttpClient"/>,
    /// options validation, and a standard resilience pipeline (retry + circuit breaker + timeout).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate that configures the <see cref="SearxngOptions"/> for this client.</param>
    /// <returns>The original <paramref name="services"/> instance to enable method chaining.</returns>
    /// <remarks>
    /// <see cref="SearxngOptions"/> are validated eagerly at application startup via
    /// <c>ValidateOnStart</c>. The <see cref="HttpClient"/> timeout is set to
    /// <see cref="Timeout.InfiniteTimeSpan"/> so that the resilience pipeline's
    /// attempt timeout takes precedence rather than the hard client timeout.
    /// </remarks>
    public static IServiceCollection AddSearxngClient(this IServiceCollection services, Action<SearxngOptions> configure)
    {
        services.AddOptions<SearxngOptions>()
            .Configure(configure)
            .Validate(
                o => o.BaseUri is not null && o.BaseUri.IsAbsoluteUri,
                "BaseUri must be set to an absolute URI.")
            .Validate(
                o => o.Timeout > TimeSpan.Zero && o.Timeout <= TimeSpan.FromMinutes(5),
                "Timeout must be between 0 and 5 minutes.")
            .ValidateOnStart();

        services.AddHttpClient<IUrlRetriever<SearchRequestOptions>, SearxngClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<SearxngOptions>>().Value;

            http.BaseAddress = options.BaseUri;
            // Let the resilience pipeline own timeouts; prevent HttpClient's
            // hard timeout from racing against the pipeline's attempt timeout.
            http.Timeout = Timeout.InfiniteTimeSpan;

            if (!string.IsNullOrWhiteSpace(options.UserAgent))
                http.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        .AddStandardResilienceHandler();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IUrlRetriever{TSearchOptions}"/> using a pre-configured <see cref="SearxngOptions"/>
    /// instance, with a standard resilience pipeline (retry + circuit breaker + timeout).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="options">A fully configured <see cref="SearxngOptions"/> instance.</param>
    /// <returns>The original <paramref name="services"/> instance to enable method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddSearxngClient(this IServiceCollection services, SearxngOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return services.AddSearxngClient(o =>
        {
            o.BaseUri = options.BaseUri;
            o.Timeout = options.Timeout;
            o.SearchPath = options.SearchPath;
            o.UserAgent = options.UserAgent;
            o.ProbeInstanceOnStartup = options.ProbeInstanceOnStartup;
        });
    }
}
