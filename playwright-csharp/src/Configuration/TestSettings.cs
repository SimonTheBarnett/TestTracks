namespace TestTracks.Playwright.CSharp.Configuration;

/// <summary>
/// Runtime settings that affect browser/API behavior for a test run.
/// </summary>
public sealed record TestSettings(
    BrowserName Browser,
    bool Headless,
    int DefaultTimeoutMs,
    int ExpectTimeoutMs,
    bool TraceOnFailure,
    string ArtifactsDirectory);
