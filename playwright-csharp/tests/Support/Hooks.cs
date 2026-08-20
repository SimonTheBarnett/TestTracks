using Reqnroll;
using Reqnroll.Infrastructure;
using TestTracks.Playwright.CSharp.Specs.Reporting;

namespace TestTracks.Playwright.CSharp.Specs.Support;

[Binding]
public sealed class Hooks
{
    private readonly ScenarioState _state;
    private readonly ScenarioContext _scenarioContext;
    private readonly ReqnrollOutputHelper _outputHelper;

    public Hooks(
        ScenarioState state,
        ScenarioContext scenarioContext,
        ReqnrollOutputHelper outputHelper)
    {
        _state = state;
        _scenarioContext = scenarioContext;
        _outputHelper = outputHelper;
    }

    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        var failed = _scenarioContext.TestError is not null;
        var evidence = new EvidenceSupport(_state, _outputHelper);
        var teardownFailures = new List<Exception>();

        try
        {
            await evidence.AttachAsync(failed);
        }
        catch (Exception ex)
        {
            teardownFailures.Add(new InvalidOperationException("Evidence capture failed.", ex));
        }

        try
        {
            teardownFailures.AddRange(await _state.Cleanup.RunAsync());
        }
        catch (Exception ex)
        {
            teardownFailures.Add(new InvalidOperationException("Cleanup failed before all registered cleanup actions could be completed.", ex));
        }

        try
        {
            await _state.Resources.DisposeAsync();
        }
        catch (Exception ex)
        {
            teardownFailures.Add(new InvalidOperationException("Resource disposal failed.", ex));
        }

        if (teardownFailures.Count == 0)
        {
            return;
        }

        if (failed)
        {
            WriteTeardownFailures(teardownFailures);
            return;
        }

        throw new AggregateException(teardownFailures);
    }

    private void WriteTeardownFailures(IReadOnlyList<Exception> failures)
    {
        _outputHelper.WriteLine("== Teardown failures ==");

        foreach (var failure in failures)
        {
            _outputHelper.WriteLine(failure.ToString());
        }
    }
}
