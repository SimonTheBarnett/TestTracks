using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Cleanup;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;
using TestTracks.Playwright.CSharp.Runtime;
using TestTracks.Playwright.CSharp.Specs.Configuration;
using TestTracks.Playwright.CSharp.Specs.Data;

namespace TestTracks.Playwright.CSharp.Specs.Support;

public sealed class ScenarioState
{
    private string? _siteTargetName;

    public ScenarioState()
    {
        var testRun = TestRunConfiguration.Current;

        ScenarioId = TestData.NewScenarioId();
        EnvironmentName = testRun.EnvironmentName;
        Data = testRun.Data;
        Targets = Data.Load<EnvironmentTargets>("all-targets");
        Settings = testRun.Settings;
        Cleanup = new CleanupRegistry();
        ApiEvidence = new ApiEvidence();
    }

    public string ScenarioId { get; }

    public string EnvironmentName { get; }

    public EnvironmentDataStore Data { get; }

    public EnvironmentTargets Targets { get; }

    public TestSettings Settings { get; }

    public CleanupRegistry Cleanup { get; }

    public ScenarioResources Resources { get; } = new();

    public ApiEvidence ApiEvidence { get; }

    public async Task<TApi> UseApiAsync<TApi>(
        string apiName,
        Func<TestSettings, IAPIRequestContext, ApiEvidence, TApi> factory)
    {
        var apiBaseUrl = new Uri(Targets.Api(apiName).BaseUrl);
        var session = await Resources.GetOrCreateApiAsync(apiName, apiBaseUrl);
        return factory(Settings, session.Request, ApiEvidence);
    }

    public async Task<IPage> UsePageAsync(string siteName)
    {
        if (_siteTargetName == siteName && Resources.Browser is not null)
        {
            return Resources.Browser.Page;
        }

        if (Resources.Browser is not null)
        {
            throw new InvalidOperationException(
                $"This scenario is already using UI site target '{_siteTargetName}'. Multiple browser sessions are not configured in this example state.");
        }

        var siteBaseUrl = new Uri(Targets.Site(siteName).BaseUrl);
        Resources.Browser = await BrowserSession.CreateAsync(Settings, siteBaseUrl, ScenarioId);
        _siteTargetName = siteName;

        return Resources.Browser.Page;
    }
}
