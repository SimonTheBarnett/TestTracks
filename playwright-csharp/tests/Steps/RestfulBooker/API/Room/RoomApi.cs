using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.API.Common;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Room;

public sealed class RoomApi : BaseApi
{
    public RoomApi(TestSettings settings, IAPIRequestContext request, ApiEvidence? evidence = null)
        : base(settings, request, evidence)
    {
    }

    public async Task<Room> CreateRoom(
        JsonObject payload,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        await EnsureOkAsync("POST", "room", payload, HeadersWithToken(token, headers), query);

        var rooms = await GetRooms(headers: headers);
        var roomName = PayloadString(payload, "roomName");
        return rooms.Items.LastOrDefault(candidate => candidate.RoomName == roomName)
            ?? throw new InvalidOperationException($"Created room '{roomName}' was not returned by GET room.");
    }

    public async Task<Room> GetRoom(
        int roomId,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<Room>("GET", $"room/{roomId}", headers: headers, query: query);
    }

    public async Task<Rooms> GetRooms(
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<Rooms>("GET", "room", headers: headers, query: query);
    }

    public async Task DeleteRoom(
        int roomId,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        await EnsureOkAsync("DELETE", $"room/{roomId}", headers: HeadersWithToken(token, headers), query: query);
    }

    private static IReadOnlyDictionary<string, string> HeadersWithToken(
        string token,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var merged = headers is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        merged["Cookie"] = $"token={token}";
        return merged;
    }

    private static string PayloadString(JsonObject payload, string propertyName)
    {
        return payload[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"Room payload is missing '{propertyName}'.");
    }
}

public sealed record Room(
    [property: JsonPropertyName("roomid")] int RoomId,
    [property: JsonPropertyName("roomName")] string RoomName,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("accessible")] bool Accessible,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("features")] IReadOnlyList<string> Features,
    [property: JsonPropertyName("roomPrice")] int RoomPrice);

public sealed record Rooms(
    [property: JsonPropertyName("rooms")] IReadOnlyList<Room> Items);
