package testtracks.configuration;

public record EnvironmentRunSettings(
    int defaultTimeoutMs, int expectTimeoutMs, boolean traceOnFailure) {}
