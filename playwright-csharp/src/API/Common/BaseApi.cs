using System.Text.Json;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;

namespace TestTracks.Playwright.CSharp.API.Common;

/// <summary>
/// Base class for API clients that use Playwright request contexts.
/// </summary>
public abstract class BaseApi
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected BaseApi(TestSettings settings, IAPIRequestContext request, ApiEvidence? evidence = null)
    {
        Settings = settings;
        Request = request;
        Evidence = evidence;
    }

    protected TestSettings Settings { get; }

    protected IAPIRequestContext Request { get; }

    protected ApiEvidence? Evidence { get; }

    /// <summary>
    /// Sends an API request through Playwright and records the response evidence.
    /// </summary>
    protected async Task<IAPIResponse> SendAsync(
        string method,
        string path,
        object? payload = null,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        var response = await Request.FetchAsync(path, new APIRequestContextOptions
        {
            Method = method,
            DataObject = payload,
            Headers = headers,
            Params = QueryParams(query)
        });

        Evidence?.Record(Operation(method, path), response.Status, await response.TextAsync());
        return response;
    }

    /// <summary>
    /// Sends an API request, verifies it succeeded and deserializes the JSON body.
    /// </summary>
    protected async Task<T> SendJsonAsync<T>(
        string method,
        string path,
        object? payload = null,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        var response = await SendAsync(method, path, payload, headers, query);
        return await ReadJsonAsync<T>(response, Operation(method, path));
    }

    /// <summary>
    /// Sends an API request and verifies it succeeded.
    /// </summary>
    protected async Task EnsureOkAsync(
        string method,
        string path,
        object? payload = null,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        var response = await SendAsync(method, path, payload, headers, query);
        EnsureOk(response, Operation(method, path));
    }

    /// <summary>
    /// Verifies a recorded response succeeded and deserializes the JSON body.
    /// </summary>
    protected async Task<T> ReadJsonAsync<T>(IAPIResponse response, string operation)
    {
        var body = await response.TextAsync();
        EnsureOk(response, operation, body);

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new InvalidOperationException($"{operation} returned an empty JSON body.");
    }

    /// <summary>
    /// Records the response and verifies it succeeded.
    /// </summary>
    protected async Task EnsureOkAsync(IAPIResponse response, string operation)
    {
        var body = await response.TextAsync();
        EnsureOk(response, operation, body);
    }

    private static void EnsureOk(IAPIResponse response, string operation, string? body = null)
    {
        if (!response.Ok)
        {
            throw new InvalidOperationException(
                $"{operation} failed with HTTP {response.Status}: {SecretRedactor.Redact(body)}");
        }
    }

    private static string Operation(string method, string url)
    {
        return $"{method.ToUpperInvariant()} {url}";
    }

    private static Dictionary<string, object>? QueryParams(IReadOnlyDictionary<string, string?>? query)
    {
        if (query is null || query.Count == 0)
        {
            return null;
        }

        var parameters = query
            .Where(item => item.Value is not null)
            .ToDictionary(item => item.Key, item => (object)item.Value!);

        return parameters.Count == 0 ? null : parameters;
    }
}
