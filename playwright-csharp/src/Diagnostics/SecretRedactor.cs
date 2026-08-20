using System.Text.RegularExpressions;

namespace TestTracks.Playwright.CSharp.Diagnostics;

/// <summary>
/// Removes common secrets from evidence before it is written to reports or failures.
/// </summary>
public static class SecretRedactor
{
    private const string Replacement = "[REDACTED]";
    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private static readonly string[] JsonSecretFields =
    [
        "accessToken",
        "apiKey",
        "authorization",
        "clientSecret",
        "cookie",
        "password",
        "refreshToken",
        "secret",
        "sessionId",
        "token"
    ];

    private static readonly string[] KeyValueSecretNames =
    [
        "access_token",
        "api_key",
        "client_secret",
        "password",
        "refresh_token",
        "sessionid",
        "token"
    ];

    private static readonly string[] HeaderSecretNames =
    [
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key"
    ];

    private static readonly string JsonSecretFieldPattern = string.Join("|", JsonSecretFields.Select(Regex.Escape));
    private static readonly string KeyValueSecretNamePattern = string.Join("|", KeyValueSecretNames.Select(Regex.Escape));
    private static readonly string HeaderSecretNamePattern = string.Join("|", HeaderSecretNames.Select(Regex.Escape));
    private static readonly Regex JsonSecretRegex = new(
        $"""("(?:{JsonSecretFieldPattern})"\s*:\s*")([^"]*)(")""",
        Options);
    private static readonly Regex KeyValueSecretRegex = new(
        $"""((?:{KeyValueSecretNamePattern})=)[^;\s&]+""",
        Options);
    private static readonly Regex HeaderSecretRegex = new(
        $"""((?:{HeaderSecretNamePattern})\s*:\s*)[^\r\n]+""",
        Options);

    /// <summary>
    /// Redacts common secret fields, key-value pairs and headers from text.
    /// </summary>
    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var redacted = JsonSecretRegex.Replace(value, $"$1{Replacement}$3");
        redacted = KeyValueSecretRegex.Replace(redacted, $"$1{Replacement}");
        redacted = HeaderSecretRegex.Replace(redacted, $"$1{Replacement}");
        return redacted;
    }
}
