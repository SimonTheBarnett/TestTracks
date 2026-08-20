using System.Text.Json.Nodes;
using TestTracks.Playwright.CSharp.Specs.Configuration;

namespace TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;

public static class AuthPayloadBuilder
{
    public static JsonObject FromCredential(CredentialTarget credential)
    {
        return new JsonObject
        {
            ["username"] = credential.Username,
            ["password"] = credential.Password
        };
    }
}
