package testtracks.runtime;

import com.microsoft.playwright.Browser;
import com.microsoft.playwright.BrowserContext;
import com.microsoft.playwright.BrowserType;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.Playwright;
import com.microsoft.playwright.PlaywrightException;
import java.io.IOException;
import java.io.UncheckedIOException;
import java.net.URI;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import testtracks.configuration.BrowserName;
import testtracks.configuration.TestSettings;
import testtracks.diagnostics.UiEvidence;

public final class BrowserSession implements AutoCloseable {
  private final Playwright playwright;
  private final Browser browser;
  private final BrowserContext context;
  private final Page page;
  private final TestSettings settings;
  private final UiEvidence evidence;
  private boolean tracingStarted;

  private BrowserSession(
      Playwright playwright,
      Browser browser,
      BrowserContext context,
      Page page,
      TestSettings settings,
      UiEvidence evidence) {
    this.playwright = playwright;
    this.browser = browser;
    this.context = context;
    this.page = page;
    this.settings = settings;
    this.evidence = evidence;
  }

  public Page page() {
    return page;
  }

  public UiEvidence evidence() {
    return evidence;
  }

  public static BrowserSession create(TestSettings settings, URI baseUrl, String scenarioId) {
    var playwright = Playwright.create();
    var browserType =
        settings.browser() == BrowserName.FIREFOX ? playwright.firefox() : playwright.chromium();
    var launchOptions = new BrowserType.LaunchOptions().setHeadless(settings.headless());

    if (settings.browser() == BrowserName.EDGE) {
      launchOptions.setChannel("msedge");
    }

    try {
      var browser = browserType.launch(launchOptions);
      var context =
          browser.newContext(new Browser.NewContextOptions().setBaseURL(baseUrl.toString()));
      context.setDefaultTimeout(settings.defaultTimeoutMs());
      context.setDefaultNavigationTimeout(settings.defaultTimeoutMs());

      var page = context.newPage();
      page.setDefaultTimeout(settings.defaultTimeoutMs());
      page.setDefaultNavigationTimeout(settings.defaultTimeoutMs());

      var evidence = new UiEvidence();
      evidence.attachTo(page);
      var session = new BrowserSession(playwright, browser, context, page, settings, evidence);

      if (settings.traceOnFailure()) {
        context
            .tracing()
            .start(
                new com.microsoft.playwright.Tracing.StartOptions()
                    .setScreenshots(true)
                    .setSnapshots(true)
                    .setSources(true)
                    .setTitle("test-tracks-playwright-java " + scenarioId));
        session.tracingStarted = true;
      }

      return session;
    } catch (PlaywrightException ex) {
      playwright.close();
      if (settings.browser() == BrowserName.EDGE) {
        throw new IllegalStateException(
            "BROWSER=edge requires the Microsoft Edge stable channel. Install it with the Playwright browser install command for msedge.",
            ex);
      }
      throw ex;
    } catch (RuntimeException ex) {
      playwright.close();
      throw ex;
    }
  }

  public Path stopTracing(boolean saveTrace, String scenarioArtifactName) {
    if (!tracingStarted) {
      return null;
    }

    tracingStarted = false;
    if (!saveTrace) {
      context.tracing().stop();
      return null;
    }

    var tracePath = Path.of(settings.artifactsDirectory(), scenarioArtifactName, "trace.zip");
    try {
      Files.createDirectories(tracePath.getParent());
    } catch (IOException ex) {
      throw new UncheckedIOException("Could not create Playwright trace folder.", ex);
    }
    context.tracing().stop(new com.microsoft.playwright.Tracing.StopOptions().setPath(tracePath));
    return tracePath;
  }

  @Override
  public void close() {
    var failures = new ArrayList<RuntimeException>();

    // Keep closing later resources even when an earlier Playwright resource fails to dispose.
    closeCollecting("page", page::close, failures);
    closeCollecting("browser context", context::close, failures);
    closeCollecting("browser", browser::close, failures);
    closeCollecting("Playwright", playwright::close, failures);

    if (!failures.isEmpty()) {
      var failure =
          new IllegalStateException("One or more browser session resources failed to close.");
      failures.forEach(failure::addSuppressed);
      throw failure;
    }
  }

  private static void closeCollecting(
      String resourceName, Runnable action, List<RuntimeException> failures) {
    try {
      action.run();
    } catch (RuntimeException ex) {
      failures.add(new IllegalStateException("Failed to close " + resourceName + ".", ex));
    }
  }
}
