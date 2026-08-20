using System.Globalization;
using System.Text.Json.Nodes;
using TestTracks.Playwright.CSharp.Specs.Data;

namespace TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;

public sealed record RoomDataDefaults
{
    public required JsonObject Payload { get; init; }

    public int PriceMinimum { get; init; } = 300;

    public int PriceMaximumExclusive { get; init; } = 900;

    public int RoomIdMinimum { get; init; } = 7000;

    public int RoomIdMaximumExclusive { get; init; } = 9999;
}

public sealed class RoomDataBuilder
{
    private readonly JsonObject _payload;

    public RoomDataBuilder(RoomDataDefaults defaults)
    {
        _payload = defaults.Payload.DeepClone().AsObject();
    }

    public static RoomDataBuilder ForScenario(string scenarioId, RoomDataDefaults defaults)
    {
        var roomNumber = TestData.NumericSuffix(defaults.RoomIdMinimum, defaults.RoomIdMaximumExclusive);

        var builder = new RoomDataBuilder(defaults);
        builder
            .WithRoomId(roomNumber)
            .With("description", $"{builder.StringValue("description")} {scenarioId}");

        if (!builder.HasValue("roomPrice"))
        {
            builder.With("roomPrice", Random.Shared.Next(defaults.PriceMinimum, defaults.PriceMaximumExclusive));
        }

        return builder;
    }

    public RoomDataBuilder WithRoomId(int roomId)
    {
        return With("roomid", roomId)
            .With("roomName", roomId.ToString(CultureInfo.InvariantCulture));
    }

    public RoomDataBuilder WithType(string type)
    {
        return With("type", type);
    }

    public RoomDataBuilder With(string propertyName, object? value)
    {
        _payload[propertyName] = JsonValue.Create(value);
        return this;
    }

    public JsonObject BuildPayload()
    {
        return _payload.DeepClone().AsObject();
    }

    private string StringValue(string propertyName)
    {
        return _payload[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"Room payload is missing '{propertyName}'.");
    }

    private bool HasValue(string propertyName)
    {
        return _payload.ContainsKey(propertyName) && _payload[propertyName] is not null;
    }
}
