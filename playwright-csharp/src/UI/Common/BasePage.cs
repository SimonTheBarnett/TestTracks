using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;

namespace TestTracks.Playwright.CSharp.UI.Common;

/// <summary>
/// Base class for page objects backed by a Playwright page.
/// </summary>
public abstract class BasePage
{
    public IPage Page { get; }

    protected TestSettings Settings { get; }

    protected BasePage(IPage page, TestSettings settings)
    {
        Page = page;
        Settings = settings;
    }

    /// <summary>
    /// Opens a route relative to the browser context base URL.
    /// </summary>
    public async Task OpenAsync(string relativeRoute = "")
    {
        await Page.GotoAsync(relativeRoute);
    }
}
