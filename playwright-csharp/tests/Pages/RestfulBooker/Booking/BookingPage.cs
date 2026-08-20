using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.UI.Common;

namespace TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Booking;

public sealed class BookingPage : BasePage
{
    private const string ReserveNowButtonName = "Reserve Now";
    private const string BookingApiPath = "/api/booking";
    private const string BookingConfirmationTextPattern = "Booking (Successful|Confirmed)";
    private const string FirstNameLabel = "Firstname";
    private const string FirstNameLabelWithSpace = "First name";
    private const string FirstNameLabelWithCapital = "First Name";
    private const string LastNameLabel = "Lastname";
    private const string LastNameLabelWithSpace = "Last name";
    private const string LastNameLabelWithCapital = "Last Name";
    private const string EmailLabel = "Email";
    private const string PhoneLabel = "Phone";

    public ILocator Confirmation =>
        Page.GetByText(new Regex(BookingConfirmationTextPattern, RegexOptions.IgnoreCase));

    private ILocator ReserveNowButton =>
        Page.Locator("#doReservation");

    private ILocator SubmitBookingButton =>
        Page.GetByRole(
            AriaRole.Button,
            new() { Name = ReserveNowButtonName, Exact = true });

    private ILocator FieldByLabel(string label) =>
        Page.GetByLabel(label);

    public BookingPage(IPage page, TestSettings settings)
        : base(page, settings)
    {
    }

    public async Task<int> BookRoom(BookingFormData booking)
    {
        await ReserveNowButton.ClickAsync();

        await FillByPossibleLabels([FirstNameLabel, FirstNameLabelWithSpace, FirstNameLabelWithCapital], booking.FirstName);
        await FillByPossibleLabels([LastNameLabel, LastNameLabelWithSpace, LastNameLabelWithCapital], booking.LastName);
        await FillByPossibleLabels([EmailLabel], booking.Email);
        await FillByPossibleLabels([PhoneLabel], booking.Phone);

        var bookingResponse = Page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" &&
            response.Url.Contains(BookingApiPath, StringComparison.OrdinalIgnoreCase));

        await SubmitBookingButton.ClickAsync();

        var response = await bookingResponse;
        var body = await response.TextAsync();

        if (!response.Ok)
        {
            throw new InvalidOperationException($"UI booking request failed with HTTP {response.Status}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("bookingid").GetInt32();
    }

    private async Task FillByPossibleLabels(IEnumerable<string> labels, string value)
    {
        foreach (var label in labels)
        {
            var locator = FieldByLabel(label);
            if (await locator.CountAsync() > 0)
            {
                await locator.FillAsync(value);
                return;
            }
        }

        throw new InvalidOperationException($"Could not find any expected booking form label for value '{value}'.");
    }
}
