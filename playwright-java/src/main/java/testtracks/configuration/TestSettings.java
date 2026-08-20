package testtracks.configuration;

public record TestSettings(
    BrowserName browser,
    boolean headless,
    int defaultTimeoutMs,
    int expectTimeoutMs,
    boolean traceOnFailure,
    String artifactsDirectory) {}
