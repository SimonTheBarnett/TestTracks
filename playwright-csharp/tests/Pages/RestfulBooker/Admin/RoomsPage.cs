using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.UI.Common;

namespace TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Admin;

public sealed class RoomsPage : BasePage
{
    private ILocator RoomNameText(string roomName) =>
        Page.GetByText(roomName, new() { Exact = true });

    public RoomsPage(IPage page, TestSettings settings)
        : base(page, settings)
    {
    }

    public ILocator RoomNamed(string roomName)
    {
        return RoomNameText(roomName);
    }
}
