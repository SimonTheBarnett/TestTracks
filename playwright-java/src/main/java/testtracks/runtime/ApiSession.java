package testtracks.runtime;

import com.microsoft.playwright.APIRequest;
import com.microsoft.playwright.APIRequestContext;
import com.microsoft.playwright.Playwright;
import java.net.URI;
import java.util.ArrayList;
import java.util.Map;

public final class ApiSession implements AutoCloseable {
  private final Playwright playwright;
  private final APIRequestContext request;

  private ApiSession(Playwright playwright, APIRequestContext request) {
    this.playwright = playwright;
    this.request = request;
  }

  public APIRequestContext request() {
    return request;
  }

  public static ApiSession create(URI baseUrl) {
    var playwright = Playwright.create();
    var request =
        playwright
            .request()
            .newContext(
                new APIRequest.NewContextOptions()
                    .setBaseURL(baseUrl.toString())
                    .setExtraHTTPHeaders(Map.of("Accept", "application/json")));

    return new ApiSession(playwright, request);
  }

  @Override
  public void close() {
    var failures = new ArrayList<Exception>();

    try {
      request.dispose();
    } catch (Exception ex) {
      failures.add(new IllegalStateException("API request context disposal failed.", ex));
    }

    try {
      playwright.close();
    } catch (Exception ex) {
      failures.add(new IllegalStateException("API Playwright disposal failed.", ex));
    }

    if (!failures.isEmpty()) {
      throw new IllegalStateException("API session disposal failed.", failures.getFirst());
    }
  }
}
