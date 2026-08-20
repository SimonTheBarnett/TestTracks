package testtracks.runtime;

import java.net.URI;
import java.util.HashMap;
import java.util.Map;

public final class ScenarioResources implements AutoCloseable {
  private final Map<String, ApiSession> apiSessions = new HashMap<>();
  private BrowserSession browser;

  public BrowserSession browser() {
    return browser;
  }

  public void browser(BrowserSession browser) {
    this.browser = browser;
  }

  public ApiSession getOrCreateApi(String name, URI baseUrl) {
    return apiSessions.computeIfAbsent(name, ignored -> ApiSession.create(baseUrl));
  }

  @Override
  public void close() {
    var failures = new java.util.ArrayList<Exception>();

    if (browser != null) {
      try {
        browser.close();
      } catch (Exception ex) {
        failures.add(new IllegalStateException("Browser session disposal failed.", ex));
      }
    }

    for (var session : apiSessions.values()) {
      try {
        session.close();
      } catch (Exception ex) {
        failures.add(ex);
      }
    }

    if (!failures.isEmpty()) {
      throw new IllegalStateException("Scenario resource disposal failed.", failures.getFirst());
    }
  }
}
