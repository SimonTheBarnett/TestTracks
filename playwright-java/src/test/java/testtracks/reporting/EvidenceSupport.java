package testtracks.reporting;

import io.cucumber.java.Scenario;
import testtracks.support.ScenarioState;

public final class EvidenceSupport {
  private final ScenarioState state;
  private final Scenario scenario;

  public EvidenceSupport(ScenarioState state, Scenario scenario) {
    this.state = state;
    this.scenario = scenario;
  }

  public void attach(boolean failed) {
    scenario.log("== API evidence ==");
    scenario.log(state.apiEvidence().text());

    var browser = state.resources().browser();
    if (browser == null) {
      return;
    }

    scenario.log("== Browser ==");
    scenario.log(
        "Browser: "
            + state.settings().browser().name().toLowerCase()
            + System.lineSeparator()
            + "Headless: "
            + state.settings().headless());

    logIfAny("Browser console", browser.evidence().consoleMessages());
    logIfAny("Page errors", browser.evidence().pageErrors());

    if (failed) {
      var screenshot =
          browser
              .page()
              .screenshot(new com.microsoft.playwright.Page.ScreenshotOptions().setFullPage(true));
      scenario.attach(screenshot, "image/png", "Failure screenshot");

      var tracePath = browser.stopTracing(true, scenarioArtifactName());
      if (tracePath != null) {
        scenario.log("Playwright trace written to: " + tracePath);
      }
    } else {
      browser.stopTracing(false, state.scenarioId());
    }
  }

  private void logIfAny(String title, java.util.List<String> lines) {
    if (lines.isEmpty()) {
      return;
    }

    scenario.log("== " + title + " ==");
    scenario.log(String.join(System.lineSeparator(), lines));
  }

  private String scenarioArtifactName() {
    var safeName =
        scenario.getName().toLowerCase().replaceAll("[^a-z0-9]+", "-").replaceAll("(^-|-$)", "");
    if (safeName.isBlank()) {
      safeName = "scenario";
    }
    return safeName + "-" + state.scenarioId();
  }
}
