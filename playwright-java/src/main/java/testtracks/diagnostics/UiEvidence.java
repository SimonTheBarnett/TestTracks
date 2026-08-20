package testtracks.diagnostics;

import com.microsoft.playwright.Page;
import java.util.ArrayList;
import java.util.List;

public final class UiEvidence {
  private final List<String> consoleMessages = new ArrayList<>();
  private final List<String> pageErrors = new ArrayList<>();

  public List<String> consoleMessages() {
    return List.copyOf(consoleMessages);
  }

  public List<String> pageErrors() {
    return List.copyOf(pageErrors);
  }

  public void attachTo(Page page) {
    page.onConsoleMessage(
        message ->
            consoleMessages.add(SecretRedactor.redact(message.type() + ": " + message.text())));
    page.onPageError(error -> pageErrors.add(SecretRedactor.redact(error)));
  }
}
