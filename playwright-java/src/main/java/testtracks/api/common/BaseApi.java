package testtracks.api.common;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.microsoft.playwright.APIRequestContext;
import com.microsoft.playwright.APIResponse;
import com.microsoft.playwright.options.RequestOptions;
import java.util.Map;
import testtracks.configuration.TestSettings;
import testtracks.diagnostics.ApiEvidence;
import testtracks.diagnostics.SecretRedactor;

public abstract class BaseApi {
  protected static final ObjectMapper JSON = new ObjectMapper();

  protected final TestSettings settings;
  protected final APIRequestContext request;
  protected final ApiEvidence evidence;

  protected BaseApi(TestSettings settings, APIRequestContext request, ApiEvidence evidence) {
    this.settings = settings;
    this.request = request;
    this.evidence = evidence;
  }

  protected APIResponse send(
      String method,
      String path,
      JsonNode payload,
      Map<String, String> headers,
      Map<String, String> query) {
    var options = RequestOptions.create().setMethod(method);

    if (payload != null) {
      options.setData(payload.toString());
      options.setHeader("Content-Type", "application/json");
    }

    if (headers != null) {
      headers.forEach(options::setHeader);
    }

    if (query != null) {
      query.forEach(
          (name, value) -> {
            if (value != null) {
              options.setQueryParam(name, value);
            }
          });
    }

    var response = request.fetch(path, options);
    evidence.record(operation(method, path), response.status(), response.text());
    return response;
  }

  protected APIResponse send(String method, String path) {
    return send(method, path, null, null, null);
  }

  protected <T> T sendJson(
      String method,
      String path,
      JsonNode payload,
      Map<String, String> headers,
      Map<String, String> query,
      Class<T> responseType) {
    var response = send(method, path, payload, headers, query);
    return readJson(response, operation(method, path), responseType);
  }

  protected void ensureOk(
      String method,
      String path,
      JsonNode payload,
      Map<String, String> headers,
      Map<String, String> query) {
    var response = send(method, path, payload, headers, query);
    ensureOk(response, operation(method, path), response.text());
  }

  protected <T> T readJson(APIResponse response, String operation, Class<T> responseType) {
    var body = response.text();
    ensureOk(response, operation, body);
    try {
      return JSON.readValue(body, responseType);
    } catch (Exception ex) {
      throw new IllegalStateException(operation + " returned invalid JSON.", ex);
    }
  }

  protected void ensureOk(APIResponse response, String operation) {
    ensureOk(response, operation, response.text());
  }

  private static void ensureOk(APIResponse response, String operation, String body) {
    if (!response.ok()) {
      throw new IllegalStateException(
          operation
              + " failed with HTTP "
              + response.status()
              + ": "
              + SecretRedactor.redact(body));
    }
  }

  private static String operation(String method, String path) {
    return method.toUpperCase() + " " + path;
  }
}
