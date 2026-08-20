package testtracks.steps.restfulbooker.api.auth;

import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.microsoft.playwright.APIRequestContext;
import com.microsoft.playwright.APIResponse;
import java.util.Map;
import testtracks.api.common.BaseApi;
import testtracks.configuration.TestSettings;
import testtracks.diagnostics.ApiEvidence;

public final class AuthApi extends BaseApi {
  public AuthApi(TestSettings settings, APIRequestContext request, ApiEvidence evidence) {
    super(settings, request, evidence);
  }

  public AuthToken logIn(ObjectNode payload) {
    return logIn(payload, null, null);
  }

  public AuthToken logIn(
      ObjectNode payload, Map<String, String> headers, Map<String, String> query) {
    return sendJson("POST", "auth/login", payload, headers, query, AuthToken.class);
  }

  public APIResponse tryLogIn(ObjectNode payload) {
    return send("POST", "auth/login", payload, null, null);
  }

  public TokenValidation validate(String token) {
    return sendJson(
        "POST", "auth/validate", tokenPayload(token), null, null, TokenValidation.class);
  }

  public void logOut(String token) {
    ensureOk("POST", "auth/logout", tokenPayload(token), null, null);
  }

  private static ObjectNode tokenPayload(String token) {
    var payload = JsonNodeFactory.instance.objectNode();
    payload.put("token", token);
    return payload;
  }
}
