using NUnit.Framework;
using Reqnroll.Infrastructure;
using TestTracks.Playwright.CSharp.Specs.Support;

namespace TestTracks.Playwright.CSharp.Specs.Reporting;

public sealed class EvidenceSupport
{
    private readonly ScenarioState _state;
    private readonly ReqnrollOutputHelper _outputHelper;

    public EvidenceSupport(ScenarioState state, ReqnrollOutputHelper outputHelper)
    {
        _state = state;
        _outputHelper = outputHelper;
    }

    public async Task AttachAsync(bool failed)
    {
        Directory.CreateDirectory(_state.Settings.ArtifactsDirectory);

        WriteEvidence("API evidence", _state.ApiEvidence.Text());

        if (_state.Resources.Browser is null)
        {
            return;
        }

        WriteEvidence("Browser", BrowserEvidence());
        WriteEvidenceIfAny("Browser console", _state.Resources.Browser.Evidence.ConsoleMessages);
        WriteEvidenceIfAny("Page errors", _state.Resources.Browser.Evidence.PageErrors);

        if (failed)
        {
            var screenshotPath = Path.Combine(
                _state.Settings.ArtifactsDirectory,
                $"{_state.ScenarioId}-failure.png");

            await _state.Resources.Browser.Page.ScreenshotAsync(new()
            {
                Path = screenshotPath,
                FullPage = true
            });

            _outputHelper.AddAttachment(screenshotPath);
            TestContext.AddTestAttachment(screenshotPath, "Failure screenshot");

            var tracePath = await _state.Resources.Browser.StopTracingAsync(saveTrace: true, _state.ScenarioId);
            if (tracePath is not null)
            {
                _outputHelper.AddAttachment(tracePath);
                TestContext.AddTestAttachment(tracePath, "Playwright trace");
            }
        }
        else
        {
            await _state.Resources.Browser.StopTracingAsync(saveTrace: false, _state.ScenarioId);
        }
    }

    private void WriteEvidence(string title, string content)
    {
        _outputHelper.WriteLine($"== {title} ==");
        _outputHelper.WriteLine(content);
    }

    private void WriteEvidenceIfAny(string title, IReadOnlyList<string> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        WriteEvidence(title, string.Join(Environment.NewLine, lines));
    }

    private string BrowserEvidence()
    {
        var browser = _state.Settings.Browser.ToString().ToLowerInvariant();

        return string.Join(
            Environment.NewLine,
            $"Browser: {browser}",
            $"Headless: {_state.Settings.Headless.ToString().ToLowerInvariant()}");
    }
}
