package testtracks.configuration;

public record TestRunContext(
    String environmentName, TestSettings settings, EnvironmentDataStore data) {}
