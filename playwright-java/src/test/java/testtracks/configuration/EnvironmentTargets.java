package testtracks.configuration;

import java.util.Map;

public record EnvironmentTargets(
    Map<String, SiteTarget> sites,
    Map<String, ApiTarget> apis,
    Map<String, CredentialTarget> credentials,
    EnvironmentRunSettings settings) {

  public SiteTarget site(String name) {
    return target(sites, name, "site");
  }

  public ApiTarget api(String name) {
    return target(apis, name, "api");
  }

  public CredentialTarget credential(String name) {
    return target(credentials, name, "credential");
  }

  private static <T> T target(Map<String, T> values, String name, String type) {
    var value = values.get(name);
    if (value == null) {
      throw new IllegalStateException(
          "The " + type + " target '" + name + "' was not found in all-targets.json.");
    }
    return value;
  }
}
