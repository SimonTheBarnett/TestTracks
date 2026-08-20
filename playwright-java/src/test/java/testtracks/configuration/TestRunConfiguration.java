package testtracks.configuration;

public final class TestRunConfiguration {
  private static final String DEFAULT_ENVIRONMENT = "dev";
  private static final String DEFAULT_BROWSER = "chromium";
  private static final boolean DEFAULT_HEADLESS = false;
  private static final TestRunContext CURRENT = create();

  private TestRunConfiguration() {}

  public static TestRunContext current() {
    return CURRENT;
  }

  private static TestRunContext create() {
    var environmentName = runValue("ENV", DEFAULT_ENVIRONMENT);
    var browser = parseBrowser(runValue("BROWSER", DEFAULT_BROWSER));
    var headless = parseBoolean(runValue("HEADLESS", Boolean.toString(DEFAULT_HEADLESS)));
    var data = new EnvironmentDataStore(environmentName);
    var targets = data.load("all-targets", EnvironmentTargets.class);
    var settings = targets.settings();

    return new TestRunContext(
        environmentName,
        new TestSettings(
            browser,
            headless,
            settings.defaultTimeoutMs(),
            settings.expectTimeoutMs(),
            settings.traceOnFailure(),
            RunArtifacts.directory()),
        data);
  }

  private static String runValue(String name, String defaultValue) {
    var propertyValue = System.getProperty(name);
    if (propertyValue != null && !propertyValue.isBlank()) {
      return propertyValue.trim();
    }

    var environmentValue = System.getenv(name);
    return environmentValue == null || environmentValue.isBlank()
        ? defaultValue
        : environmentValue.trim();
  }

  private static BrowserName parseBrowser(String value) {
    return switch (value.trim().toLowerCase()) {
      case "chromium" -> BrowserName.CHROMIUM;
      case "firefox" -> BrowserName.FIREFOX;
      case "edge" -> BrowserName.EDGE;
      default ->
          throw new IllegalStateException(
              "Unsupported BROWSER value '" + value + "'. Use chromium, firefox or edge.");
    };
  }

  private static boolean parseBoolean(String value) {
    if ("true".equalsIgnoreCase(value) || "false".equalsIgnoreCase(value)) {
      return Boolean.parseBoolean(value);
    }
    throw new IllegalStateException("HEADLESS must be true or false.");
  }
}
