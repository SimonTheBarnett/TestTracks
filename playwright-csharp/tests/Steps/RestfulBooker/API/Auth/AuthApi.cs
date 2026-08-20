using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.API.Common;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth;

public sealed class AuthApi : BaseApi
{
    public AuthApi(TestSettings settings, IAPIRequestContext request, ApiEvidence? evidence = null)
        : base(settings, request, evidence)
    {
    }

    public async Task<AuthToken> LogIn(
        JsonObject payload,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<AuthToken>("POST", "auth/login", payload, headers, query);
    }

    public async Task<IAPIResponse> TryLogIn(
        JsonObject payload,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendAsync("POST", "auth/login", payload, headers, query);
    }

    public async Task<TokenValidation> Validate(
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<TokenValidation>("POST", "auth/validate", TokenPayload(token), headers, query);
    }

    public async Task LogOut(
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        await EnsureOkAsync("POST", "auth/logout", TokenPayload(token), headers, query);
    }

    private static JsonObject TokenPayload(string token)
    {
        return new JsonObject
        {
            ["token"] = token
        };
    }
}

public sealed record AuthToken(
    [property: JsonPropertyName("token")] string Token);

public sealed record TokenValidation(
    [property: JsonPropertyName("valid")] bool Valid);
