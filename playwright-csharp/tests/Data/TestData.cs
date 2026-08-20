using System.Security.Cryptography;

namespace TestTracks.Playwright.CSharp.Specs.Data;

public static class TestData
{
    public static string NewScenarioId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    public static int NumericSuffix(int minimum = 7000, int maximumExclusive = 9999)
    {
        return RandomNumberGenerator.GetInt32(minimum, maximumExclusive);
    }

    public static string SafeName(string prefix, string scenarioId, int maxLength)
    {
        var value = $"{prefix}_{scenarioId}";
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
