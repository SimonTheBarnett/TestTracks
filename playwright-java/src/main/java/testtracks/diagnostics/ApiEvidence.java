package testtracks.diagnostics;

import java.util.ArrayList;
import java.util.List;

public final class ApiEvidence {
  private final List<String> entries = new ArrayList<>();

  public List<String> entries() {
    return List.copyOf(entries);
  }

  public void record(String operation, int status, String body) {
    entries.add(
        operation + " -> HTTP " + status + System.lineSeparator() + SecretRedactor.redact(body));
  }

  public String text() {
    return entries.isEmpty()
        ? "No API evidence was captured."
        : String.join(System.lineSeparator() + System.lineSeparator(), entries);
  }
}
