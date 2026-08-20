using System.Text.Json.Nodes;
using NUnit.Framework;
using Reqnroll;
using TestTracks.Playwright.CSharp.Specs.Data;
using TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;
using TestTracks.Playwright.CSharp.Specs.Support;
using AuthApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth.AuthApi;
using RoomApiClient = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Room.RoomApi;
using RoomModel = TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Room.Room;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API;

[Binding]
public sealed class RoomApiSteps
{
    private const string ApiTarget = "restfulBookerApi";
    private const string AdminCredential = "restfulBookerAdmin";

    private readonly ScenarioState _state;
    private RoomModel? _currentRoom;
    private JsonObject? _currentRoomPayload;

    public RoomApiSteps(ScenarioState state)
    {
        _state = state;
    }

    [Given("valid room details")]
    public async Task GivenValidRoomDetails()
    {
        var data = _state.Data.Load<RoomApiTestData>(
            "api-room",
            "scenarios.validRoomDetails");

        _currentRoomPayload = RoomDataBuilder
            .ForScenario(_state.ScenarioId, data.Room)
            .BuildPayload();
    }

    [When("the room is created through the room API")]
    public async Task WhenTheRoomIsCreatedThroughTheRoomApi()
    {
        var admin = _state.Targets.Credential(AdminCredential);
        var authApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new AuthApiClient(settings, request, evidence));
        var roomApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new RoomApiClient(settings, request, evidence));

        var authToken = await authApi.LogIn(AuthPayloadBuilder.FromCredential(admin));

        var created = await roomApi.CreateRoom(_currentRoomPayload!, authToken.Token);
        _currentRoom = created;

        _state.Cleanup.Register(
            $"Delete room {created.RoomId}",
            () => roomApi.DeleteRoom(created.RoomId, authToken.Token));
    }

    [Then("the room can be retrieved with the same details")]
    public async Task ThenTheRoomCanBeRetrievedWithTheSameDetails()
    {
        var roomApi = await _state.UseApiAsync(
            ApiTarget,
            (settings, request, evidence) => new RoomApiClient(settings, request, evidence));

        var retrieved = await roomApi.GetRoom(_currentRoom!.RoomId);
        Assert.That(retrieved.RoomName, Is.EqualTo(_currentRoom.RoomName));
        Assert.That(retrieved.Type, Is.EqualTo(_currentRoom.Type));
    }

}

public sealed record RoomApiTestData(RoomDataDefaults Room);
