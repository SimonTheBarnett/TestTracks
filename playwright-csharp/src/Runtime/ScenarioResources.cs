namespace TestTracks.Playwright.CSharp.Runtime;

/// <summary>
/// Owns the disposable Playwright resources created during one test scenario.
/// </summary>
public sealed class ScenarioResources : IAsyncDisposable
{
    private readonly Dictionary<string, ApiSession> _apis = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Uri> _apiBaseUrls = new(StringComparer.OrdinalIgnoreCase);

    public BrowserSession? Browser { get; set; }

    /// <summary>
    /// Creates or returns a named API session for this scenario.
    /// </summary>
    /// <remarks>
    /// Use separate names when one scenario needs to talk to multiple APIs with different base URLs.
    /// Reusing a name with a different base URL is treated as a test setup error.
    /// </remarks>
    public async Task<ApiSession> GetOrCreateApiAsync(string name, Uri baseUrl)
    {
        if (_apis.TryGetValue(name, out var existing))
        {
            if (_apiBaseUrls.TryGetValue(name, out var existingBaseUrl) && existingBaseUrl != baseUrl)
            {
                throw new InvalidOperationException(
                    $"API session '{name}' already exists for '{existingBaseUrl}', so it cannot be reused for '{baseUrl}'.");
            }

            return existing;
        }

        var created = await ApiSession.CreateAsync(baseUrl);
        _apis.Add(name, created);
        _apiBaseUrls.Add(name, baseUrl);
        return created;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        var failures = new List<Exception>();

        if (Browser is not null)
        {
            try
            {
                await Browser.DisposeAsync();
            }
            catch (Exception ex)
            {
                failures.Add(new InvalidOperationException("Browser resource disposal failed.", ex));
            }
        }

        foreach (var api in _apis)
        {
            try
            {
                await api.Value.DisposeAsync();
            }
            catch (Exception ex)
            {
                failures.Add(new InvalidOperationException($"API resource disposal failed: {api.Key}", ex));
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException(failures);
        }
    }
}
