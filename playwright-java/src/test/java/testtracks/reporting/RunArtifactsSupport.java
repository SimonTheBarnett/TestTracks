package testtracks.reporting;

import io.cucumber.java.BeforeAll;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import testtracks.configuration.RunArtifacts;
import testtracks.configuration.TestRunConfiguration;

public final class RunArtifactsSupport {
  private static boolean shutdownHookRegistered;
  private static boolean formatterOutputsMoved;

  private RunArtifactsSupport() {}

  @BeforeAll
  public static void beforeTestRun() throws IOException {
    Files.createDirectories(Path.of(RunArtifacts.directory()));

    if (!shutdownHookRegistered) {
      Runtime.getRuntime().addShutdownHook(new Thread(RunArtifactsSupport::moveFormatterOutputs));
      shutdownHookRegistered = true;
    }
  }

  private static synchronized void moveFormatterOutputs() {
    if (formatterOutputsMoved) {
      return;
    }

    try {
      moveIfExists(
          RunArtifacts.rootDirectory().resolve("test-report.html"), RunArtifacts.reportHtmlPath());
      moveIfExists(
          RunArtifacts.rootDirectory().resolve("cucumber-messages.ndjson"),
          RunArtifacts.cucumberMessagesPath());
      injectRunDetailsIntoReport();
      formatterOutputsMoved = true;
    } catch (Exception ignored) {
    }
  }

  private static void moveIfExists(Path sourcePath, Path destinationPath) throws IOException {
    if (!Files.exists(sourcePath)) {
      return;
    }

    Files.createDirectories(destinationPath.getParent());
    Files.move(sourcePath, destinationPath, java.nio.file.StandardCopyOption.REPLACE_EXISTING);
  }

  private static void injectRunDetailsIntoReport() throws IOException {
    var report = RunArtifacts.reportHtmlPath();
    if (!Files.exists(report)) {
      return;
    }

    var html = Files.readString(report);
    if (html.contains("id=\"test-tracks-playwright-java-run-details\"")) {
      return;
    }

    var bodyIndex = html.toLowerCase().indexOf("<body>");
    if (bodyIndex < 0) {
      return;
    }

    var insertAt = bodyIndex + "<body>".length();
    Files.writeString(
        report,
        html.substring(0, insertAt)
            + System.lineSeparator()
            + runDetailsHtml()
            + html.substring(insertAt));
  }

  private static String runDetailsHtml() {
    var testRun = TestRunConfiguration.current();
    var settings = testRun.settings();
    return
"""
<section id="test-tracks-playwright-java-run-details" style="font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 16px; padding: 12px 16px; border: 1px solid #d0d7de; border-radius: 6px; background: #f6f8fa; color: #24292f;">
  <strong>Run details</strong>
  <span style="display: inline-block; margin-left: 16px;">Environment: %s</span>
  <span style="display: inline-block; margin-left: 16px;">Browser: %s</span>
  <span style="display: inline-block; margin-left: 16px;">Headless: %s</span>
</section>
"""
        .formatted(
            escape(testRun.environmentName()),
            escape(settings.browser().name().toLowerCase()),
            escape(Boolean.toString(settings.headless())));
  }

  private static String escape(String value) {
    return value
        .replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace("\"", "&quot;");
  }
}
