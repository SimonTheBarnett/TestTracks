using Microsoft.Playwright;

namespace TestTracks.Playwright.CSharp.Diagnostics;

/// <summary>
/// Captures browser console messages and page errors for a UI scenario.
/// </summary>
public sealed class UiEvidence
{
    private readonly List<string> _consoleMessages = [];
    private readonly List<string> _pageErrors = [];

    public IReadOnlyList<string> ConsoleMessages => _consoleMessages;
    public IReadOnlyList<string> PageErrors => _pageErrors;

    /// <summary>
    /// Subscribes to Playwright page events so UI evidence is captured as the scenario runs.
    /// </summary>
    public void AttachTo(IPage page)
    {
        page.Console += (_, message) =>
            _consoleMessages.Add(SecretRedactor.Redact($"{message.Type}: {message.Text}"));

        page.PageError += (_, error) =>
            _pageErrors.Add(SecretRedactor.Redact(error));
    }
}
