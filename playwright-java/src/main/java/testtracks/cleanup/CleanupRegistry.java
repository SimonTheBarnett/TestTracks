package testtracks.cleanup;

import java.util.ArrayList;
import java.util.List;
import java.util.Stack;

public final class CleanupRegistry {
  private final Stack<CleanupAction> actions = new Stack<>();

  public void register(String description, CleanupActionBody action) {
    actions.push(new CleanupAction(description, action));
  }

  public List<Exception> run() {
    var failures = new ArrayList<Exception>();

    while (!actions.empty()) {
      var cleanup = actions.pop();
      try {
        cleanup.action().run();
      } catch (Exception ex) {
        failures.add(new IllegalStateException("Cleanup failed: " + cleanup.description(), ex));
      }
    }

    return failures;
  }

  @FunctionalInterface
  public interface CleanupActionBody {
    void run() throws Exception;
  }

  private record CleanupAction(String description, CleanupActionBody action) {}
}
