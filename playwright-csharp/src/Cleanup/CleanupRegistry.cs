namespace TestTracks.Playwright.CSharp.Cleanup;

/// <summary>
/// Stores cleanup actions for data created by a scenario.
/// </summary>
public sealed class CleanupRegistry
{
    private readonly Stack<CleanupAction> _actions = new();

    /// <summary>
    /// Registers a cleanup action to run after the scenario.
    /// </summary>
    /// <remarks>
    /// Cleanups run in reverse order, which usually matches how dependent test data should be removed.
    /// </remarks>
    public void Register(string description, Func<Task> action)
    {
        _actions.Push(new CleanupAction(description, action));
    }

    /// <summary>
    /// Runs all registered cleanup actions and returns any failures without stopping at the first one.
    /// </summary>
    public async Task<IReadOnlyList<Exception>> RunAsync()
    {
        var failures = new List<Exception>();

        while (_actions.TryPop(out var cleanup))
        {
            try
            {
                await cleanup.Action();
            }
            catch (Exception ex)
            {
                failures.Add(new InvalidOperationException(
                    $"Cleanup failed: {cleanup.Description}", ex));
            }
        }

        return failures;
    }

    private sealed record CleanupAction(string Description, Func<Task> Action);
}
