using System.Globalization;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.UI.Common;

namespace TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Booking;

public sealed class HomePage : BasePage
{
    public HomePage(IPage page, TestSettings settings)
        : base(page, settings)
    {
    }

    public async Task OpenBookingForRoom(int roomId, BookingDateRange bookingDates)
    {
        var checkIn = bookingDates.CheckIn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var checkOut = bookingDates.CheckOut.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        await OpenAsync($"reservation/{roomId}?checkin={checkIn}&checkout={checkOut}");
    }
}
