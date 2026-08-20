using System.Text.Json;

namespace TestTracks.Playwright.CSharp.Specs.Configuration;

public sealed class EnvironmentDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EnvironmentDataStore(string environmentName)
    {
        EnvironmentName = environmentName;
        EnvironmentDirectory = GetEnvironmentDirectory(environmentName);
    }

    public string EnvironmentName { get; }

    private string EnvironmentDirectory { get; }

    public T Load<T>(string fileName, string? sectionPath = null)
    {
        var path = Path.Combine(EnvironmentDirectory, $"{fileName}.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Missing environment test data file: {path}");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var selected = SelectSection(document.RootElement, sectionPath, path);
        return selected.Deserialize<T>(JsonOptions)
            ?? throw new InvalidOperationException($"Environment test data was empty or invalid: {path}");
    }

    private static string GetEnvironmentDirectory(string environmentName)
    {
        if (environmentName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            environmentName.Contains(Path.DirectorySeparatorChar) ||
            environmentName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException(
                $"Invalid environment argument '{environmentName}'. Use a folder name under tests/Data/Environments.");
        }

        var environmentsRoot = Path.Combine(AppContext.BaseDirectory, "Data", "Environments");
        var environmentDirectory = Path.Combine(environmentsRoot, environmentName);
        if (Directory.Exists(environmentDirectory))
        {
            return environmentDirectory;
        }

        var available = Directory.Exists(environmentsRoot)
            ? string.Join(", ", Directory.EnumerateDirectories(environmentsRoot).Select(Path.GetFileName).Order())
            : "none";

        throw new InvalidOperationException(
            $"Environment '{environmentName}' was not found. Available environments: {available}.");
    }

    private static JsonElement SelectSection(JsonElement root, string? sectionPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sectionPath))
        {
            return root;
        }

        var current = root;
        foreach (var segment in sectionPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!current.TryGetProperty(segment, out current))
            {
                throw new InvalidOperationException(
                    $"Section '{sectionPath}' was not found in environment test data file: {filePath}");
            }
        }

        return current;
    }
}
