using System.Globalization;

namespace TestTracks.Playwright.CSharp.Configuration;

/// <summary>
/// Provides the timestamped folder paths used for reports, traces and screenshots.
/// </summary>
public static class RunArtifacts
{
    private static readonly Lazy<string> RootDirectoryPath = new(CreateRootDirectory);
    private static readonly Lazy<string> RunDirectory = new(CreateRunDirectory);

    /// <summary>
    /// The unique artifacts directory for the current test run.
    /// </summary>
    public static string Directory => RunDirectory.Value;

    /// <summary>
    /// The parent directory that contains all timestamped test result folders.
    /// </summary>
    public static string RootDirectory => RootDirectoryPath.Value;

    public static string ReportHtmlPath => Path.Combine(Directory, "test-tracks-playwright-csharp-report.html");

    public static string CucumberMessagesPath => Path.Combine(Directory, "cucumber-messages.ndjson");

    internal static string CreateFolderName(DateTimeOffset timestamp)
    {
        return timestamp.UtcDateTime.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
    }

    private static string CreateRunDirectory()
    {
        return Path.Combine(RootDirectory, CreateFolderName(DateTimeOffset.UtcNow));
    }

    private static string CreateRootDirectory()
    {
        return Path.Combine(ProjectRoot(), "TestResults");
    }

    private static string ProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "test-tracks-playwright-csharp.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find test-tracks-playwright-csharp.sln above {AppContext.BaseDirectory}.");
    }
}
