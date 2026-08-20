using System.Text.Json.Nodes;

namespace TestTracks.Playwright.CSharp.Specs.Data;

public static class JsonPayloadExtensions
{
    public static string StringValue(this JsonObject payload, string propertyName)
    {
        return payload[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"Payload is missing '{propertyName}'.");
    }

    public static int IntValue(this JsonObject payload, string propertyName)
    {
        return payload[propertyName]?.GetValue<int>()
            ?? throw new InvalidOperationException($"Payload is missing '{propertyName}'.");
    }
}
