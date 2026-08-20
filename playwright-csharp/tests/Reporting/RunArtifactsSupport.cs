using System.Net;
using Reqnroll;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Specs.Configuration;

namespace TestTracks.Playwright.CSharp.Specs.Reporting;

[Binding]
public static class RunArtifactsSupport
{
    private static readonly Lock Sync = new();
    private static bool _formatterOutputsMoved;
    private static bool _processExitRegistered;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Directory.CreateDirectory(RunArtifacts.Directory);

        if (_processExitRegistered)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) => MoveFormatterOutputs();
        _processExitRegistered = true;
    }

    private static void MoveFormatterOutputs()
    {
        lock (Sync)
        {
            if (_formatterOutputsMoved)
            {
                return;
            }

            MoveIfExists(Path.Combine(RunArtifacts.RootDirectory, "test-tracks-playwright-csharp-report.html"), RunArtifacts.ReportHtmlPath);
            MoveIfExists(Path.Combine(RunArtifacts.RootDirectory, "cucumber-messages.ndjson"), RunArtifacts.CucumberMessagesPath);
            InjectRunDetailsIntoReport();
            DeletePlaywrightScratchArtifacts();
            _formatterOutputsMoved = true;
        }
    }

    private static void MoveIfExists(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        File.Move(sourcePath, destinationPath, overwrite: true);
    }

    private static void InjectRunDetailsIntoReport()
    {
        if (!File.Exists(RunArtifacts.ReportHtmlPath))
        {
            return;
        }

        var html = File.ReadAllText(RunArtifacts.ReportHtmlPath);
        if (html.Contains("id=\"test-tracks-playwright-csharp-run-details\"", StringComparison.Ordinal))
        {
            return;
        }

        var bodyIndex = html.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
        if (bodyIndex < 0)
        {
            return;
        }

        var insertAt = bodyIndex + "<body>".Length;
        var updated = html.Insert(insertAt, Environment.NewLine + BuildRunDetailsHtml());
        File.WriteAllText(RunArtifacts.ReportHtmlPath, updated);
    }

    private static string BuildRunDetailsHtml()
    {
        var testRun = TestRunConfiguration.Current;
        var settings = testRun.Settings;
        var browser = settings.Browser.ToString().ToLowerInvariant();

        return $$"""
<section id="test-tracks-playwright-csharp-run-details" style="font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 16px; padding: 12px 16px; border: 1px solid #d0d7de; border-radius: 6px; background: #f6f8fa; color: #24292f;">
  <strong>Run details</strong>
  <span style="display: inline-block; margin-left: 16px;">Environment: {{Encode(testRun.EnvironmentName)}}</span>
  <span style="display: inline-block; margin-left: 16px;">Browser: {{Encode(browser)}}</span>
  <span style="display: inline-block; margin-left: 16px;">Headless: {{Encode(settings.Headless.ToString().ToLowerInvariant())}}</span>
</section>
""";
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private static void DeletePlaywrightScratchArtifacts()
    {
        DeleteFiles("*.trace");
        DeleteFiles("*.network");
        DeleteDirectory(Path.Combine(RunArtifacts.Directory, "resources"));
    }

    private static void DeleteFiles(string searchPattern)
    {
        foreach (var path in Directory.EnumerateFiles(RunArtifacts.Directory, searchPattern, SearchOption.TopDirectoryOnly))
        {
            TryDelete(() => File.Delete(path));
        }
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            TryDelete(() => Directory.Delete(path, recursive: true));
        }
    }

    private static void TryDelete(Action action)
    {
        try
        {
            action();
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
