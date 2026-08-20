namespace TestTracks.Playwright.CSharp.Diagnostics;

/// <summary>
/// Captures API request evidence that can be written into the test report.
/// </summary>
public sealed class ApiEvidence
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    /// <summary>
    /// Records an API operation with the response status and a redacted response body.
    /// </summary>
    public void Record(string operation, int status, string? body)
    {
        _entries.Add($"{operation} -> HTTP {status}{Environment.NewLine}{SecretRedactor.Redact(body)}");
    }

    public string Text()
    {
        return _entries.Count == 0
            ? "No API evidence was captured."
            : string.Join($"{Environment.NewLine}{Environment.NewLine}", _entries);
    }
}
