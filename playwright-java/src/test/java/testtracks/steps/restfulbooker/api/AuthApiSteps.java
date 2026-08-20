package testtracks.steps.restfulbooker.api;

import com.fasterxml.jackson.databind.node.ObjectNode;
import com.microsoft.playwright.APIResponse;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import org.junit.jupiter.api.Assertions;
import testtracks.steps.restfulbooker.api.auth.AuthApi;
import testtracks.steps.restfulbooker.api.auth.AuthToken;
import testtracks.support.ScenarioState;

public final class AuthApiSteps {
  private static final String AUTH_API_TARGET = "restfulBookerApi";

  private final ScenarioState state;
  private AuthToken authToken;
  private APIResponse authResponse;

  public AuthApiSteps(ScenarioState state) {
    this.state = state;
  }

  @When("valid admin credentials are submitted to the auth API")
  public void validAdminCredentialsAreSubmittedToTheAuthApi() {
    var data =
        state
            .data()
            .load(
                "api-auth", "scenarios.validAdminCredentialsProduceAToken", AuthApiTestData.class);

    var authApi = state.useApi(AUTH_API_TARGET, AuthApi::new);
    authToken = authApi.logIn(data.payload());
  }

  @Then("a reusable auth token is returned")
  public void aReusableAuthTokenIsReturned() {
    Assertions.assertNotNull(authToken);
    Assertions.assertFalse(authToken.token().isBlank());

    var authApi = state.useApi(AUTH_API_TARGET, AuthApi::new);
    var validation = authApi.validate(authToken.token());
    Assertions.assertTrue(validation.valid());
  }

  @When("invalid admin credentials are submitted to the auth API")
  public void invalidAdminCredentialsAreSubmittedToTheAuthApi() {
    var data =
        state
            .data()
            .load(
                "api-auth", "scenarios.invalidAdminCredentialsAreRejected", AuthApiTestData.class);

    var authApi = state.useApi(AUTH_API_TARGET, AuthApi::new);
    authResponse = authApi.tryLogIn(data.payload());
  }

  @Then("the credentials are rejected")
  public void theCredentialsAreRejected() {
    Assertions.assertNotNull(authResponse);
    Assertions.assertFalse(authResponse.ok());
  }

  public record AuthApiTestData(ObjectNode payload) {}
}
