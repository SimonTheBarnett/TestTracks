using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Specs.Reporting;

namespace TestTracks.Playwright.CSharp.Specs.Configuration;

public sealed record TestRunContext(
    string EnvironmentName,
    TestSettings Settings,
    EnvironmentDataStore Data);

public static class TestRunConfiguration
{
    private const string DefaultEnvironment = "dev";
    private const string DefaultBrowser = "chromium";
    private const bool DefaultHeadless = false;

    private static readonly Lazy<TestRunContext> CurrentRun = new(Create);

    public static TestRunContext Current => CurrentRun.Value;

    private static TestRunContext Create()
    {
        var environmentName = GetRunValue("ENV", DefaultEnvironment);
        var browser = ParseBrowser(GetRunValue("BROWSER", DefaultBrowser));
        var headless = ParseBool(GetRunValue("HEADLESS", DefaultHeadless.ToString()));
        var data = new EnvironmentDataStore(environmentName);
        var targets = data.Load<EnvironmentTargets>("all-targets");
        var settings = targets.Settings;

        return new TestRunContext(
            environmentName,
            new TestSettings(
                browser,
                headless,
                settings.DefaultTimeoutMs,
                settings.ExpectTimeoutMs,
                settings.TraceOnFailure,
                RunArtifacts.Directory),
            data);
    }

    private static string GetRunValue(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static BrowserName ParseBrowser(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "chromium" => BrowserName.Chromium,
            "firefox" => BrowserName.Firefox,
            "edge" => BrowserName.Edge,
            _ => throw new InvalidOperationException(
                $"Unsupported BROWSER value '{value}'. Use chromium, firefox or edge.")
        };
    }

    private static bool ParseBool(string value)
    {
        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException("HEADLESS must be true or false.");
    }
}
