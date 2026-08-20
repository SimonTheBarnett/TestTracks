using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;

namespace TestTracks.Playwright.CSharp.Runtime;

/// <summary>
/// Wraps a Playwright API request context and the Playwright instance that owns it.
/// </summary>
public sealed class ApiSession : IAsyncDisposable
{
    private ApiSession(IPlaywright playwright, IAPIRequestContext request)
    {
        Playwright = playwright;
        Request = request;
    }

    public IPlaywright Playwright { get; }

    public IAPIRequestContext Request { get; }

    /// <summary>
    /// Creates an API request context with a shared base URL and JSON-friendly default headers.
    /// </summary>
    public static async Task<ApiSession> CreateAsync(Uri baseUrl)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = baseUrl.ToString(),
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Accept"] = "application/json"
            }
        });

        return new ApiSession(playwright, request);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        var failures = new List<Exception>();

        try
        {
            await Request.DisposeAsync();
        }
        catch (Exception ex)
        {
            failures.Add(new InvalidOperationException("API request context disposal failed.", ex));
        }

        try
        {
            Playwright.Dispose();
        }
        catch (Exception ex)
        {
            failures.Add(new InvalidOperationException("API Playwright disposal failed.", ex));
        }

        if (failures.Count > 0)
        {
            throw new AggregateException(failures);
        }
    }
}
