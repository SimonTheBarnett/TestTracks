package testtracks.support;

import com.microsoft.playwright.APIRequestContext;
import com.microsoft.playwright.Page;
import java.net.URI;
import testtracks.cleanup.CleanupRegistry;
import testtracks.configuration.EnvironmentDataStore;
import testtracks.configuration.EnvironmentTargets;
import testtracks.configuration.TestRunConfiguration;
import testtracks.configuration.TestSettings;
import testtracks.data.TestData;
import testtracks.diagnostics.ApiEvidence;
import testtracks.runtime.BrowserSession;
import testtracks.runtime.ScenarioResources;

public final class ScenarioState {
  private String siteTargetName;

  public ScenarioState() {
    var testRun = TestRunConfiguration.current();
    scenarioId = TestData.newScenarioId();
    environmentName = testRun.environmentName();
    data = testRun.data();
    targets = data.load("all-targets", EnvironmentTargets.class);
    settings = testRun.settings();
  }

  private final String scenarioId;
  private final String environmentName;
  private final EnvironmentDataStore data;
  private final EnvironmentTargets targets;
  private final TestSettings settings;
  private final CleanupRegistry cleanup = new CleanupRegistry();
  private final ScenarioResources resources = new ScenarioResources();
  private final ApiEvidence apiEvidence = new ApiEvidence();

  public String scenarioId() {
    return scenarioId;
  }

  public String environmentName() {
    return environmentName;
  }

  public EnvironmentDataStore data() {
    return data;
  }

  public EnvironmentTargets targets() {
    return targets;
  }

  public TestSettings settings() {
    return settings;
  }

  public CleanupRegistry cleanup() {
    return cleanup;
  }

  public ScenarioResources resources() {
    return resources;
  }

  public ApiEvidence apiEvidence() {
    return apiEvidence;
  }

  public <TApi> TApi useApi(String apiName, ApiFactory<TApi> factory) {
    var apiBaseUrl = URI.create(targets.api(apiName).baseUrl());
    var session = resources.getOrCreateApi(apiName, apiBaseUrl);
    return factory.create(settings, session.request(), apiEvidence);
  }

  public Page usePage(String siteName) {
    if (siteName.equals(siteTargetName) && resources.browser() != null) {
      return resources.browser().page();
    }

    if (resources.browser() != null) {
      throw new IllegalStateException(
          "This scenario is already using UI site target '"
              + siteTargetName
              + "'. Multiple browser sessions are not configured in this example state.");
    }

    var siteBaseUrl = URI.create(targets.site(siteName).baseUrl());
    resources.browser(BrowserSession.create(settings, siteBaseUrl, scenarioId));
    siteTargetName = siteName;

    return resources.browser().page();
  }

  @FunctionalInterface
  public interface ApiFactory<TApi> {
    TApi create(TestSettings settings, APIRequestContext request, ApiEvidence evidence);
  }
}
