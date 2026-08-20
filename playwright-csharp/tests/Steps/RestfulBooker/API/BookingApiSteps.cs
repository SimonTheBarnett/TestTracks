using System.Text.Json.Nodes;
using NUnit.Framework;
using Reqnroll;
using TestTracks.Playwright.CSharp.Specs.Data;
using TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;
using TestTracks.Playwright.CSharp.Specs.Support;
using AuthApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth.AuthApi;
using AuthToken = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth.AuthToken;
using BookingApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Booking.BookingApi;
using CreatedBooking = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Booking.CreatedBooking;
using RoomApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Room.RoomApi;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API;

[Binding]
public sealed class BookingApiSteps
{
    private const string ApiTarget = "restfulBookerApi";
    private const string AdminCredential = "restfulBookerAdmin";

    private readonly ScenarioState _state;
    private AuthToken? _authToken;
    private JsonObject? _bookingPayload;
    private CreatedBooking? _createdBooking;

    public BookingApiSteps(ScenarioState state)
    {
        _state = state;
    }

    [Given("valid booking details")]
    public async Task GivenValidBookingDetails()
    {
        var data = _state.Data.Load<BookingApiTestData>(
            "api-booking",
            "scenarios.validBookingDetails");

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
        _state.Cleanup.Register(
            $"Delete room {createdRoom.RoomId}",
            () => roomApi.DeleteRoom(createdRoom.RoomId, _authToken!.Token));

        _bookingPayload = BookingDataBuilder
            .ForScenario(_state.ScenarioId, data.Booking)
            .ForRoom(createdRoom.RoomId)
            .BuildPayload();
    }

    [When("the booking is created through the booking API")]
    public async Task WhenTheBookingIsCreatedThroughTheBookingApi()
    {
        var bookingApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new BookingApiClient(settings, request, evidence));

        _createdBooking = await bookingApi.CreateBooking(
            _bookingPayload!,
            _authToken!.Token);

        _state.Cleanup.Register(
            $"Delete booking {_createdBooking.BookingId}",
            () => bookingApi.DeleteBooking(_createdBooking!.BookingId, _authToken!.Token));
    }

    [Then("the booking can be retrieved with the same details")]
    public async Task ThenTheBookingCanBeRetrievedWithTheSameDetails()
    {
        var bookingPayload = _bookingPayload
            ?? throw new InvalidOperationException("Booking payload was not created.");

        var bookingApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new BookingApiClient(settings, request, evidence));

        var retrieved = await bookingApi.GetBooking(
            _createdBooking!.BookingId,
            _authToken!.Token);

        Assert.That(retrieved.FirstName, Is.EqualTo(bookingPayload.StringValue("firstname")));
        Assert.That(retrieved.LastName, Is.EqualTo(bookingPayload.StringValue("lastname")));
        Assert.That(retrieved.RoomId, Is.EqualTo(bookingPayload.IntValue("roomid")));
    }
}

public sealed record BookingApiTestData(
    BookingDataDefaults Booking,
    RoomDataDefaults Room);
