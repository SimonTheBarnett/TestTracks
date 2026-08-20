using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;
using TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Admin;
using TestTracks.Playwright.CSharp.Specs.Pages.RestfulBooker.Booking;
using TestTracks.Playwright.CSharp.Specs.Support;
using AuthApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth.AuthApi;
using AuthToken = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth.AuthToken;
using BookingApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Booking.BookingApi;
using RoomApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Room.RoomApi;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.UI;

[Binding]
public sealed class BookingSteps
{
    private const string SiteTarget = "restfulBooker";
    private const string ApiTarget = "restfulBookerApi";
    private const string AdminCredential = "restfulBookerAdmin";

    private readonly ScenarioState _state;
    private BookingPage? _bookingPage;
    private RoomsPage? _roomsPage;
    private AuthToken? _authToken;
    private int? _currentRoomId;
    private string? _currentRoomName;
    private int? _createdBookingId;

    public BookingSteps(ScenarioState state)
    {
        _state = state;
    }

    [Given("an available room exists")]
    public async Task GivenAnAvailableRoomExists()
    {
        var data = _state.Data.Load<AvailableRoomUiTestData>("ui-booking", "scenarios.availableRoom");

        var authApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new AuthApiClient(settings, request, evidence));
        var roomApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new RoomApiClient(settings, request, evidence));

        var admin = _state.Targets.Credential(AdminCredential);
        _authToken = await authApi.LogIn(AuthPayloadBuilder.FromCredential(admin));

        var room = RoomDataBuilder
            .ForScenario(_state.ScenarioId, data.Room)
            .BuildPayload();
        var createdRoom = await roomApi.CreateRoom(room, _authToken.Token);

        _currentRoomId = createdRoom.RoomId;
        _currentRoomName = createdRoom.RoomName;
        _state.Cleanup.Register(
            $"Delete room {createdRoom.RoomId}",
            () => roomApi.DeleteRoom(createdRoom.RoomId, _authToken!.Token));
    }

    [When("the guest books the room")]
    public async Task WhenTheGuestBooksTheRoom()
    {
        var data = _state.Data.Load<GuestBookingUiTestData>("ui-booking", "scenarios.guestCanCreateBooking");
        var page = await _state.UsePageAsync(SiteTarget);

        var homePage = new HomePage(page, _state.Settings);
        _bookingPage = new BookingPage(page, _state.Settings);
        var bookingApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new BookingApiClient(settings, request, evidence));

        var bookingDetails = BookingDataBuilder
            .ForScenario(_state.ScenarioId, data.Booking)
            .ForRoom(_currentRoomId!.Value)
            .BuildFormData();

        await homePage.OpenBookingForRoom(_currentRoomId.Value, bookingDetails.BookingDates);
        _createdBookingId = await _bookingPage.BookRoom(bookingDetails);

        _state.Cleanup.Register(
            $"Delete booking {_createdBookingId.Value}",
            () => bookingApi.DeleteBooking(_createdBookingId.Value, _authToken!.Token));
    }

    [Then("the booking is shown as confirmed")]
    public async Task ThenTheBookingIsShownAsConfirmed()
    {
        await Assertions.Expect(_bookingPage!.Confirmation).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = _state.Settings.ExpectTimeoutMs });

        Assert.That(_createdBookingId, Is.GreaterThan(0));
    }

    [When("an administrator views the rooms")]
    public async Task WhenAnAdministratorViewsTheRooms()
    {
        var admin = _state.Targets.Credential(AdminCredential);
        var page = await _state.UsePageAsync(SiteTarget);

        var adminLoginPage = new AdminLoginPage(page, _state.Settings);
        _roomsPage = new RoomsPage(page, _state.Settings);

        await adminLoginPage.LogIn(
            admin.Username,
            admin.Password);
    }

    [Then("the room is visible to the administrator")]
    public async Task ThenTheRoomIsVisibleToTheAdministrator()
    {
        await Assertions.Expect(_roomsPage!.RoomNamed(_currentRoomName!)).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = _state.Settings.ExpectTimeoutMs });
    }

}

public sealed record AvailableRoomUiTestData(RoomDataDefaults Room);

public sealed record GuestBookingUiTestData(BookingDataDefaults Booking);
