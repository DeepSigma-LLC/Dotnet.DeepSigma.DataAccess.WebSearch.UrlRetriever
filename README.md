# DeepSigma.DataAccess.WebSearch.UrlRetriever

A typed, production-ready .NET client library for [SearXNG](https://docs.searxng.org/dev/search_api.html) — the self-hostable, privacy-respecting metasearch engine. Built on `IHttpClientFactory`, source-generated `System.Text.Json`, and `Microsoft.Extensions.Http.Resilience` for a clean, testable, DI-friendly integration.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![NuGet](https://img.shields.io/nuget/v/DeepSigma.DataAccess.WebSearch.UrlRetriever.svg)](https://www.nuget.org/packages/DeepSigma.DataAccess.WebSearch.UrlRetriever)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
  - [SearxngOptions Reference](#searxngoptions-reference)
  - [Registering with Dependency Injection](#registering-with-dependency-injection)
  - [Manual Construction (no DI)](#manual-construction-no-di)
- [Making Requests](#making-requests)
  - [SearchRequestOptions Reference](#searchrequestoptions-reference)
  - [SafeSearchLevel Enum](#safesearchlevel-enum)
- [Reading Responses](#reading-responses)
  - [SearchResponse Reference](#searchresponse-reference)
  - [SearchResult Reference](#searchresult-reference)
  - [SearchMetadata Reference](#searchmetadata-reference)
  - [SearchWarning Reference](#searchwarning-reference)
- [Error Handling](#error-handling)
  - [Exception Hierarchy](#exception-hierarchy)
  - [Exception Reference](#exception-reference)
- [Resilience](#resilience)
- [Observability](#observability)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Contributing](#contributing)

---

## Features

- **Strongly typed** request and response models — no raw JSON leaks into application code
- **Provider-neutral interface** (`IUrlRetriever<SearchRequestOptions>`) from `DeepSigma.DataAccess.WebSearch.Abstraction` — designed for future backend extensibility
- **Built-in resilience** via `Microsoft.Extensions.Http.Resilience` — retry, circuit breaker, and attempt timeout out of the box
- **Source-generated JSON** deserialization via `System.Text.Json` — reflection-free and AOT/trim-safe
- **Typed exceptions** with a clean hierarchy that maps HTTP and network conditions to actionable error types
- **Options validation** at startup — misconfigured instances fail fast rather than at the first request
- **DI-first** — registers as a typed `HttpClient` via `IHttpClientFactory`; also usable in console apps without a DI container
- **Structured logging** — query latency and result counts emitted through `ILogger`

---

## Requirements

| Component | Version |
|---|---|
| .NET | 10.0+ |
| SearXNG instance | Any version with `format=json` enabled |

> **Important:** JSON format support must be enabled on your SearXNG instance. In the instance's settings, ensure `json` is included in `search.formats`. Public instances may have this disabled — for production use, run your own instance.

---

## Installation

### .NET CLI

```shell
dotnet add package DeepSigma.DataAccess.WebSearch.UrlRetriever
```

### Package Manager Console

```powershell
Install-Package DeepSigma.DataAccess.WebSearch.UrlRetriever
```

---

## Quick Start

### 1. Register the client

```csharp
// Program.cs (ASP.NET Core / Generic Host)
builder.Services.AddSearxngClient(options =>
{
    options.BaseUri   = new Uri("https://your-searxng-instance.example.com");
    options.Timeout   = TimeSpan.FromSeconds(10);
    options.UserAgent = "MyApp/1.0";
});
```

### 2. Inject and search

```csharp
using DeepSigma.DataAccess.WebSearch.Abstraction;
using DeepSigma.DataAccess.WebSearch.Abstraction.Model;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;

public class SearchService(IUrlRetriever<SearchRequestOptions> searxng)
{
    public async Task<List<ResponseUrlRetrival>> FindAsync(
        string query,
        CancellationToken ct = default)
    {
        return await searxng.SearchAsync(query,
            new SearchRequestOptions(Language: "en"), ct);
    }
}
```

### 3. Handle errors

```csharp
try
{
    var results = await searxng.SearchAsync("open source");

    foreach (var result in results)
        Console.WriteLine($"{result.Title} — {result.Url}");
}
catch (SearxngUnsupportedFormatException)
{
    // JSON is disabled on this SearXNG instance — check search.formats in instance settings
}
catch (SearxngUnavailableException ex)
{
    logger.LogError(ex, "SearXNG instance is unreachable");
}
catch (SearxngException ex)
{
    // Catch-all for any other SearXNG-specific failure
    logger.LogError(ex, "Search failed");
}
```

---

## Configuration

### `SearxngOptions` Reference

| Property | Type | Default | Required | Description |
|---|---|---|---|---|
| `BaseUri` | `Uri?` | — | ✅ | Absolute base URI of the SearXNG instance, e.g. `https://searxng.example.com`. Must be non-null at startup. |
| `Timeout` | `TimeSpan` | `00:00:10` | | Per-attempt timeout. Must be between `TimeSpan.Zero` and `00:05:00`. |
| `SearchPath` | `string` | `"/search"` | | Path to the SearXNG search endpoint. |
| `UserAgent` | `string?` | `null` | | Value sent as the `User-Agent` request header. Recommended when hitting public instances. |

Validation rules enforced eagerly at application startup:

- `BaseUri` must be non-null and an absolute URI.
- `Timeout` must be greater than `TimeSpan.Zero` and at most 5 minutes.
- `SearchPath` must be non-empty and start with `/`.

### Registering with Dependency Injection

`AddSearxngClient` registers `IUrlRetriever<SearchRequestOptions>` as a typed `HttpClient`, validates `SearxngOptions` eagerly on startup, and attaches a standard resilience pipeline. Two calling styles are supported:

**Configure via delegate** (recommended for `appsettings.json` integration):

```csharp
services.AddSearxngClient(options =>
{
    options.BaseUri   = new Uri(builder.Configuration["SearXNG:BaseUri"]!);
    options.Timeout   = TimeSpan.FromSeconds(8);
    options.UserAgent = "MyApp/1.0 (+https://myapp.example.com)";
});
```

**Pass a pre-built instance** (useful when options are constructed in code):

```csharp
services.AddSearxngClient(new SearxngOptions
{
    BaseUri   = new Uri("https://searxng.example.com"),
    Timeout   = TimeSpan.FromSeconds(8),
    UserAgent = "MyApp/1.0 (+https://myapp.example.com)"
});
```

Options can also be bound from `appsettings.json`:

```json
{
  "SearXNG": {
    "BaseUri": "https://searxng.example.com",
    "Timeout": "00:00:08",
    "UserAgent": "MyApp/1.0"
  }
}
```

```csharp
services.AddSearxngClient(options =>
    builder.Configuration.GetSection("SearXNG").Bind(options));
```

### Manual Construction (no DI)

> **Note:** Manual construction skips the resilience pipeline that `AddSearxngClient` attaches (retry, circuit breaker, attempt timeout). Without that pipeline, `HttpClient.Timeout` is what actually governs request duration; `SearxngOptions.Timeout` has no runtime effect. For production use, prefer the DI registration above.

```csharp
using DeepSigma.DataAccess.WebSearch.UrlRetriever;
using DeepSigma.DataAccess.WebSearch.UrlRetriever.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://searxng.example.com"),
    Timeout     = TimeSpan.FromSeconds(10)
};

var options = new SearxngOptions
{
    BaseUri   = new Uri("https://searxng.example.com"),
    UserAgent = "MyConsoleApp/1.0"
};

var client = new SearxngClient(
    httpClient,
    Options.Create(options),
    NullLogger<SearxngClient>.Instance);
```

---

## Making Requests

### `SearchRequestOptions` Reference

`SearchRequestOptions` is an immutable positional record. All parameters are optional and fall back to the SearXNG instance's own defaults when omitted. The query string is passed separately to `SearchAsync`.

```csharp
var requestOptions = new SearchRequestOptions(
    Page:       2,
    Language:   "en",
    TimeRange:  "year",
    SafeSearch: SafeSearchLevel.Moderate,
    Categories: ["science", "news"],
    Engines:    ["google", "bing"]);

List<ResponseUrlRetrival> results = await client.SearchAsync("climate change research", requestOptions);
```

| Parameter | Type | Default | SearXNG field | Description |
|---|---|---|---|---|
| `Page` | `int?` | `null` | `pageno` | One-based page number for pagination. `null` fetches the first page. |
| `Language` | `string?` | `null` | `language` | BCP-47 language/region code, e.g. `en`, `en-US`, `fr`. |
| `TimeRange` | `string?` | `null` | `time_range` | Time filter: `day`, `week`, `month`, or `year`. |
| `SafeSearch` | `SafeSearchLevel?` | `null` | `safesearch` | Safe-search filtering level. `null` defers to the instance default. |
| `Categories` | `IReadOnlyList<string>?` | `null` | `categories` | Category names to scope the search, e.g. `general`, `images`, `news`. |
| `Engines` | `IReadOnlyList<string>?` | `null` | `engines` | Engine names to restrict the search to specific providers, e.g. `google`, `bing`. |

### `SafeSearchLevel` Enum

| Value | Integer sent | Description |
|---|---|---|
| `Off` | `0` | Safe-search filtering is disabled. |
| `Moderate` | `1` | Moderate safe-search filtering is applied. |
| `Strict` | `2` | Strict safe-search filtering is applied. |

---

## Reading Responses

### `SearchResponse` Reference

```csharp
// Use SearchRawAsync on SearxngClient directly for the rich SearchResponse
SearchResponse response = await client.SearchRawAsync("open source", requestOptions);

Console.WriteLine(
    $"Found {response.Metadata.ResultCount} results on this page " +
    $"({response.Metadata.TotalResults} total) " +
    $"in {response.Metadata.Duration.TotalMilliseconds:F0} ms");

foreach (SearchResult result in response.Results)
    Console.WriteLine($"[{result.Engine}] {result.Title} — {result.Url}");

foreach (string answer in response.Answers)
    Console.WriteLine($"Direct answer: {answer}");

foreach (string suggestion in response.Suggestions)
    Console.WriteLine($"Suggestion: {suggestion}");

foreach (SearchWarning warning in response.Warnings)
    Console.WriteLine($"Warning [{warning.Engine}]: {warning.Message}");
```

| Property | Type | Description |
|---|---|---|
| `Results` | `IReadOnlyList<SearchResult>` | Ordered list of search results. Never `null`; may be empty. |
| `Metadata` | `SearchMetadata` | Metadata about the request, timing, and result count. |
| `Warnings` | `IReadOnlyList<SearchWarning>` | Non-fatal warnings populated from unresponsive engines. Never `null`; may be empty. |
| `Answers` | `IReadOnlyList<string>` | Direct answers generated by SearXNG or a contributing engine (e.g. calculator results). Never `null`; may be empty. |
| `Corrections` | `IReadOnlyList<string>` | Spelling corrections returned by SearXNG. Never `null`; may be empty. |
| `Suggestions` | `IReadOnlyList<string>` | Related query suggestions returned by SearXNG. Never `null`; may be empty. |

### `SearchResult` Reference

| Property | Type | Description |
|---|---|---|
| `Title` | `string` | Display title of the result page. Empty string if not provided by SearXNG. |
| `Url` | `string` | Canonical URL of the result page. Always non-null and non-empty. |
| `Snippet` | `string?` | Short excerpt from the page content (SearXNG `content` field). `null` if absent. |
| `Engine` | `string?` | Primary contributing search engine, e.g. `google`, `bing`. `null` if not reported. |
| `Engines` | `IReadOnlyList<string>?` | All engines that returned this result when SearXNG de-duplicates across providers. |
| `Score` | `double?` | Aggregated relevance score assigned by SearXNG based on per-engine positions. Higher is more relevant. |
| `Category` | `string?` | SearXNG category this result belongs to, e.g. `general`, `news`, `images`. |
| `PublishedDate` | `DateTimeOffset?` | Publication or last-modified date of the page, if reported by the engine. |
| `PrettyUrl` | `string?` | Human-readable, shortened form of the URL suitable for display. |
| `Template` | `string?` | Result template type reported by SearXNG. Common values: `default.html`, `images.html`, `videos.html`, `torrent.html`, `map.html`. |
| `Thumbnail` | `string?` | URL of a thumbnail image. Present on image, video, and news results. |
| `ImageUrl` | `string?` | URL of the full-size image for image search results. |
| `Author` | `string?` | Author or byline of the result page. Typically present on news and article results. |
| `IframeSrc` | `string?` | URL of an embeddable iframe for video results. |
| `ParsedUrls` | `IReadOnlyList<string>?` | Additional URLs extracted from the result content. `null` if not provided. |

### `SearchMetadata` Reference

| Property | Type | Description |
|---|---|---|
| `InstanceBaseUrl` | `string` | Base URL of the SearXNG instance that handled the request. |
| `Query` | `string` | The original query string that was submitted. |
| `Page` | `int?` | Page number requested, or `null` if the default first page was used. |
| `Duration` | `TimeSpan` | Total elapsed time from request dispatch to response mapping. |
| `Partial` | `bool` | `true` when one or more engines were unresponsive and the result set may be incomplete. |
| `ResultCount` | `int` | Number of items in `SearchResponse.Results` for the current page. |
| `TotalResults` | `long?` | Total number of results reported by SearXNG across all pages. `null` if not provided by the instance. |

### `SearchWarning` Reference

| Property | Type | Description |
|---|---|---|
| `Message` | `string` | Human-readable description of the warning, formatted as `"engine: errorType"` for unresponsive engines. |
| `Engine` | `string?` | Name of the engine that triggered the warning. `null` for non-engine warnings. |
| `ErrorCode` | `string?` | Error type reported by SearXNG, e.g. `HTTP error`, `timeout`, `no results`. `null` if not applicable. |

---

## Error Handling

All exceptions thrown by `SearxngClient` derive from `SearxngException`. Catch specific types for granular handling, or catch the abstract base for a single catch-all.

### Exception Hierarchy

```
Exception
└── SearxngException                        (abstract base — catch-all for library errors)
    ├── SearxngUnavailableException          (network failure — DNS, connection refused, etc.)
    ├── SearxngTimeoutException              (request exceeded configured timeout)
    ├── SearxngBadRequestException           (HTTP 4xx other than 403 — exposes StatusCode)
    ├── SearxngUnsupportedFormatException    (HTTP 403 — JSON format disabled on instance)
    └── SearxngParseException                (response body could not be deserialized)
```

### Exception Reference

| Exception | Trigger | Notable member |
|---|---|---|
| `SearxngUnavailableException` | DNS failure, connection refused, or host unreachable | `InnerException` → `HttpRequestException` |
| `SearxngTimeoutException` | Request exceeded `SearxngOptions.Timeout` | `InnerException` → `TaskCanceledException` |
| `SearxngBadRequestException` | HTTP 400–499 response (except 403) | `StatusCode` — the exact HTTP status integer |
| `SearxngUnsupportedFormatException` | HTTP 403 — JSON format disabled on the instance | — |
| `SearxngParseException` | Empty response body or structurally invalid JSON | `InnerException` → `JsonException` when applicable |

```csharp
try
{
    var results = await client.SearchAsync("query", requestOptions);
}
catch (SearxngBadRequestException ex)
{
    logger.LogWarning("Bad request — HTTP {StatusCode}", ex.StatusCode);
}
catch (SearxngUnsupportedFormatException)
{
    logger.LogError("JSON format is disabled on this SearXNG instance. Check search.formats in instance settings.");
}
catch (SearxngTimeoutException ex)
{
    logger.LogWarning(ex, "Search timed out");
}
catch (SearxngUnavailableException ex)
{
    logger.LogError(ex, "SearXNG instance unreachable");
}
catch (SearxngParseException ex)
{
    logger.LogError(ex, "Unexpected response format from SearXNG");
}
```

---

## Resilience

When registered via `AddSearxngClient`, the `HttpClient` is automatically wrapped with [`Microsoft.Extensions.Http.Resilience`](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)'s standard pipeline:

| Strategy | Behavior |
|---|---|
| **Attempt timeout** | Cancels individual attempts that exceed the configured duration |
| **Retry** | Retries transient failures (5xx, 408, network faults) with exponential back-off |
| **Circuit breaker** | Opens the circuit after sustained failures and blocks further requests until the instance recovers |
| **Total timeout** | Caps the overall duration across all retry attempts |

`HttpClient.Timeout` is set to `Timeout.InfiniteTimeSpan` so the resilience pipeline's attempt timeout governs cancellation rather than racing against the hard client timeout.

---

## Observability

### Structured Logging

`SearxngClient` emits structured log entries across the request lifecycle:

| Level | When | Message |
|---|---|---|
| `Debug` | Each successful search | `SearXNG search completed in {ElapsedMs} ms with {Count} results` |
| `Warning` | Network failure / instance unreachable (maps to `SearxngUnavailableException`) | `SearXNG request failed` |
| `Error` | Response body could not be deserialized (maps to `SearxngParseException`) | `Failed to parse SearXNG response` |

The success log is emitted at `Debug` to keep per-query volume out of default `Information`-level sinks. Enable it in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "DeepSigma.DataAccess.WebSearch.UrlRetriever.SearxngClient": "Debug"
    }
  }
}
```

### OpenTelemetry

The standard resilience pipeline emits telemetry for retry attempts, circuit breaker transitions, and attempt timeouts via Polly's built-in telemetry support, which integrates with any OpenTelemetry-compatible exporter registered in your application.

---

## Project Structure

```
DeepSigma.DataAccess.WebSearch.UrlRetriever/
├── Models/
│   ├── SearchRequestOptions.cs              # Immutable input record
│   ├── SearchResponse.cs                    # Top-level response record
│   ├── SearchResult.cs                      # Per-result record
│   ├── SearchMetadata.cs                    # Request timing and instance info
│   ├── SearchWarning.cs                     # Non-fatal warning record
│   └── SafeSearchLevel.cs                   # Enum for safesearch parameter
├── Exceptions/
│   ├── SearxngException.cs                  # Abstract base exception
│   ├── SearxngUnavailableException.cs       # Network-level failures
│   ├── SearxngTimeoutException.cs           # Request timeout
│   ├── SearxngBadRequestException.cs        # HTTP 4xx (exposes StatusCode)
│   ├── SearxngUnsupportedFormatException.cs # HTTP 403 / JSON disabled
│   └── SearxngParseException.cs             # Deserialization failures
├── Configuration/
│   └── SearxngOptions.cs                    # Strongly-typed options class
├── Internal/
│   ├── Dto/
│   │   ├── SearxngJsonResponse.cs           # Top-level JSON DTO
│   │   └── SearxngJsonResult.cs             # Per-result JSON DTO
│   ├── JsonContext.cs                        # Source-generated JsonSerializerContext
│   ├── SearxngQueryBuilder.cs               # Query string encoder
│   └── SearxngResponseMapper.cs             # DTO → domain model mapper
├── Extensions/
│   └── ServiceCollectionExtensions.cs       # AddSearxngClient DI extension
└── SearxngClient.cs                         # IUrlRetriever<SearchRequestOptions> implementation

DeepSigma.DataAccess.WebSearch.UrlRetriever.Test/
├── FakeHttpMessageHandler.cs                # HttpMessageHandler test double
├── QueryBuilderTests.cs                     # Query encoding unit tests
├── ResponseMappingTests.cs                  # DTO → domain mapping unit tests
├── SearxngClientTests.cs                    # End-to-end client unit tests
└── LiveTest.cs                              # Live integration tests (requires SearXNG instance)
```

---

## Testing

The test suite uses [xUnit v3](https://xunit.net/) and runs entirely in-process with a `FakeHttpMessageHandler` — no live SearXNG instance is required for the unit tests.

```shell
dotnet test --filter "Category!=Live"
```

### Test coverage

| Suite | Tests | What is covered |
|---|---|---|
| `QueryBuilderTests` | 8 | Query string encoding, all optional parameters, omission of null/empty values, percent-encoding of special characters |
| `ResponseMappingTests` | 11 | Valid result mapping, null result list, URL filtering (blank URLs excluded), metadata population, empty title fallback, `TotalResults` from `number_of_results`, answers/corrections/suggestions mapping, null lists → empty collections, unresponsive engines → `Partial=true` and populated warnings, media fields (template, thumbnail, image URL, author, iframe) |
| `SearxngClientTests` | 10 | Successful response, empty results, null/empty/whitespace query guards, HTTP 403, HTTP 4xx, network failure, malformed JSON, metadata round-trip |

### Running live tests

Live tests require a local SearXNG instance. The default SearXNG Docker image ships with JSON format **disabled**, which causes HTTP 403 on API requests. Use the provided `docker-compose.yml` and `searxng-settings.yml` to start a correctly configured instance:

```shell
docker compose up
```

This starts SearXNG at `http://localhost:8080` with `json` added to `search.formats`. Once running, execute the live tests:

```shell
dotnet test --filter "Category=Live"
```

Live tests skip automatically when no instance is reachable and skip with an informational message when the instance is running but has JSON format disabled — they never cause a false failure in CI.

---

## Contributing

1. Fork the repository and create a feature branch from `main`.
2. Ensure `dotnet build` and `dotnet test` pass with zero errors and zero warnings.
3. Keep the public API surface provider-neutral — avoid SearXNG-specific leakage into `IUrlRetriever<SearchRequestOptions>` or the model types.
4. Add XML documentation comments (`///`) for any new public or internal members.
5. Open a pull request against `main` with a clear description of the change and its motivation.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

*Built by [DeepSigma LLC](https://github.com/DeepSigma-LLC)*
