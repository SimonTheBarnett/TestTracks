using System.Text.RegularExpressions;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.UI.Common;

namespace TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Admin;

public sealed class AdminLoginPage : BasePage
{
    private const string AdminRoute = "admin";
    private const string UsernameLabel = "Username";
    private const string PasswordLabel = "Password";
    private const string LoginButtonName = "Login";

    private ILocator Username => Page.GetByLabel(new Regex(UsernameLabel, RegexOptions.IgnoreCase));

    private ILocator Password => Page.GetByLabel(new Regex(PasswordLabel, RegexOptions.IgnoreCase));

    private ILocator LoginButton =>
        Page.GetByRole(
            AriaRole.Button,
            new() { NameRegex = new Regex(LoginButtonName, RegexOptions.IgnoreCase) });

    public AdminLoginPage(IPage page, TestSettings settings)
        : base(page, settings)
    {
    }

    public async Task LogIn(string username, string password)
    {
        await OpenAsync(AdminRoute);
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
