using System.Text.Json.Nodes;
using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Auth;
using TestTracks.Playwright.CSharp.Specs.Support;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API;

[Binding]
public sealed class AuthApiSteps
{
    private const string AuthApiTarget = "restfulBookerApi";

    private readonly ScenarioState _state;
    private AuthToken? _authToken;
    private IAPIResponse? _authResponse;

    public AuthApiSteps(ScenarioState state)
    {
        _state = state;
    }

    [When("valid admin credentials are submitted to the auth API")]
    public async Task WhenValidAdminCredentialsAreSubmittedToTheAuthApi()
    {
        var data = _state.Data.Load<ValidAuthApiTestData>(
            "api-auth",
            "scenarios.validAdminCredentialsProduceAToken");

        var authApi = await _state.UseApiAsync(
            AuthApiTarget,
            (settings, request, evidence) => new AuthApi(settings, request, evidence));

        _authToken = await authApi.LogIn(data.Payload);
    }

    [Then("a reusable auth token is returned")]
    public async Task ThenAReusableAuthTokenIsReturned()
    {
        Assert.That(_authToken?.Token, Is.Not.Null.And.Not.Empty);
        var authApi = await _state.UseApiAsync(
            AuthApiTarget,
            (settings, request, evidence) => new AuthApi(settings, request, evidence));

        var validation = await authApi.Validate(_authToken!.Token);
        Assert.That(validation.Valid, Is.True);
    }

    [When("invalid admin credentials are submitted to the auth API")]
    public async Task WhenInvalidAdminCredentialsAreSubmittedToTheAuthApi()
    {
        var data = _state.Data.Load<InvalidAuthApiTestData>(
            "api-auth",
            "scenarios.invalidAdminCredentialsAreRejected");

        var authApi = await _state.UseApiAsync(
            AuthApiTarget,
            (settings, request, evidence) => new AuthApi(settings, request, evidence));

        _authResponse = await authApi.TryLogIn(data.Payload);
    }

    [Then("the credentials are rejected")]
    public void ThenTheCredentialsAreRejected()
    {
        Assert.That(_authResponse, Is.Not.Null);
        Assert.That(_authResponse!.Ok, Is.False);
    }
}

public sealed record ValidAuthApiTestData(JsonObject Payload);

public sealed record InvalidAuthApiTestData(JsonObject Payload);
