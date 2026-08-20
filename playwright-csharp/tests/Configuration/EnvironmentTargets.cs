namespace TestTracks.Playwright.CSharp.Specs.Configuration;

public sealed record EnvironmentTargets(
    IReadOnlyDictionary<string, SiteTarget> Sites,
    IReadOnlyDictionary<string, ApiTarget> Apis,
    IReadOnlyDictionary<string, CredentialTarget> Credentials,
    EnvironmentRunSettings Settings)
{
    public SiteTarget Site(string name) => GetTarget(Sites, name, "site");

    public ApiTarget Api(string name) => GetTarget(Apis, name, "api");

    public CredentialTarget Credential(string name) => GetTarget(Credentials, name, "credential");

    private static T GetTarget<T>(IReadOnlyDictionary<string, T> values, string name, string type)
    {
        return values.TryGetValue(name, out var value)
            ? value
            : throw new InvalidOperationException($"The {type} target '{name}' was not found in all-targets.json.");
    }
}

public sealed record SiteTarget(string BaseUrl);

public sealed record ApiTarget(string BaseUrl);

public sealed record CredentialTarget(string Username, string Password);

public sealed record EnvironmentRunSettings(
    int DefaultTimeoutMs,
    int ExpectTimeoutMs,
    bool TraceOnFailure);
