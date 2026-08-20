using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;

namespace TestTracks.Playwright.CSharp.Runtime;

/// <summary>
/// Wraps a browser, browser context and page for one UI scenario.
/// </summary>
public sealed class BrowserSession : IAsyncDisposable
{
    private bool _tracingStarted;

    private BrowserSession(
        IPlaywright playwright,
        IBrowser browser,
        IBrowserContext context,
        IPage page,
        TestSettings settings,
        UiEvidence evidence)
    {
        Playwright = playwright;
        Browser = browser;
        Context = context;
        Page = page;
        Settings = settings;
        Evidence = evidence;
    }

    public IPlaywright Playwright { get; }

    public IBrowser Browser { get; }

    public IBrowserContext Context { get; }

    public IPage Page { get; }

    public TestSettings Settings { get; }

    public UiEvidence Evidence { get; }

    /// <summary>
    /// Starts a browser session against the supplied site base URL.
    /// </summary>
    /// <remarks>
    /// Each call creates a fresh browser context, so cookies, storage and page state are not shared between scenarios.
    /// </remarks>
    public static async Task<BrowserSession> CreateAsync(TestSettings settings, Uri baseUrl, string scenarioId)
    {
        Directory.CreateDirectory(settings.ArtifactsDirectory);

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browserType = settings.Browser == BrowserName.Firefox
            ? playwright.Firefox
            : playwright.Chromium;

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless
        };

        if (settings.Browser == BrowserName.Edge)
        {
            launchOptions.Channel = "msedge";
        }

        try
        {
            var browser = await browserType.LaunchAsync(launchOptions);
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = baseUrl.ToString()
            });

            context.SetDefaultTimeout(settings.DefaultTimeoutMs);
            context.SetDefaultNavigationTimeout(settings.DefaultTimeoutMs);

            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(settings.DefaultTimeoutMs);
            page.SetDefaultNavigationTimeout(settings.DefaultTimeoutMs);

            var evidence = new UiEvidence();
            evidence.AttachTo(page);

            var session = new BrowserSession(playwright, browser, context, page, settings, evidence);

            if (settings.TraceOnFailure)
            {
                await context.Tracing.StartAsync(new TracingStartOptions
                {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true,
                    Title = $"test-tracks-playwright-csharp {scenarioId}"
                });
                session._tracingStarted = true;
            }

            return session;
        }
        catch (PlaywrightException ex) when (settings.Browser == BrowserName.Edge)
        {
            playwright.Dispose();
            throw new InvalidOperationException(
                "browser=edge requires the Microsoft Edge stable channel. Install it with the Playwright browser install command for msedge.",
                ex);
        }
        catch
        {
            playwright.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Stops Playwright tracing and optionally writes the trace zip to the run artifacts folder.
    /// </summary>
    public async Task<string?> StopTracingAsync(bool saveTrace, string scenarioId)
    {
        if (!_tracingStarted)
        {
            return null;
        }

        _tracingStarted = false;

        if (!saveTrace)
        {
            await Context.Tracing.StopAsync();
            return null;
        }

        var tracePath = Path.Combine(Settings.ArtifactsDirectory, $"{scenarioId}-trace.zip");
        await Context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = tracePath
        });
        return tracePath;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeQuietlyAsync(async () => await Page.CloseAsync());
        await DisposeQuietlyAsync(async () => await Context.CloseAsync());
        await DisposeQuietlyAsync(async () => await Browser.CloseAsync());
        Playwright.Dispose();
    }

    private static async Task DisposeQuietlyAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch
        {
        }
    }
}
