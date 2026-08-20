package testtracks.support;

import io.cucumber.java.After;
import io.cucumber.java.Scenario;
import java.util.ArrayList;
import testtracks.reporting.EvidenceSupport;

public final class Hooks {
  private final ScenarioState state;

  public Hooks(ScenarioState state) {
    this.state = state;
  }

  @After
  public void afterScenario(Scenario scenario) {
    var failed = scenario.isFailed();
    var teardownFailures = new ArrayList<Exception>();

    try {
      new EvidenceSupport(state, scenario).attach(failed);
    } catch (Exception ex) {
      teardownFailures.add(new IllegalStateException("Evidence capture failed.", ex));
    }

    try {
      teardownFailures.addAll(state.cleanup().run());
    } catch (Exception ex) {
      teardownFailures.add(
          new IllegalStateException(
              "Cleanup failed before all registered cleanup actions could be completed.", ex));
    }

    try {
      state.resources().close();
    } catch (Exception ex) {
      teardownFailures.add(new IllegalStateException("Resource disposal failed.", ex));
    }

    if (teardownFailures.isEmpty()) {
      return;
    }

    if (failed) {
      scenario.log("== Teardown failures ==");
      teardownFailures.forEach(failure -> scenario.log(failure.toString()));
      return;
    }

    throw new IllegalStateException("Teardown failed.", teardownFailures.getFirst());
  }
}
