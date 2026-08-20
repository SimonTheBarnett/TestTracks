package testtracks.steps.restfulbooker.api;

import com.fasterxml.jackson.databind.node.ObjectNode;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import org.junit.jupiter.api.Assertions;
import testtracks.data.builders.restfulbooker.AuthPayloadBuilder;
import testtracks.data.builders.restfulbooker.RoomDataBuilder;
import testtracks.data.builders.restfulbooker.RoomDataDefaults;
import testtracks.steps.restfulbooker.api.auth.AuthApi;
import testtracks.steps.restfulbooker.api.room.Room;
import testtracks.steps.restfulbooker.api.room.RoomApi;
import testtracks.support.ScenarioState;

public final class RoomApiSteps {
  private static final String API_TARGET = "restfulBookerApi";
  private static final String ADMIN_CREDENTIAL = "restfulBookerAdmin";

  private final ScenarioState state;
  private Room currentRoom;
  private ObjectNode currentRoomPayload;

  public RoomApiSteps(ScenarioState state) {
    this.state = state;
  }

  @Given("valid room details")
  public void validRoomDetails() {
    var data = state.data().load("api-room", "scenarios.validRoomDetails", RoomApiTestData.class);
    currentRoomPayload =
        RoomDataBuilder.forScenario(state.scenarioId(), data.room()).buildPayload();
  }

  @When("the room is created through the room API")
  public void theRoomIsCreatedThroughTheRoomApi() {
    var admin = state.targets().credential(ADMIN_CREDENTIAL);
    var authApi = state.useApi(API_TARGET, AuthApi::new);
    var roomApi = state.useApi(API_TARGET, RoomApi::new);

    var authToken = authApi.logIn(AuthPayloadBuilder.fromCredential(admin));
    currentRoom = roomApi.createRoom(currentRoomPayload, authToken.token());

    state
        .cleanup()
        .register(
            "Delete room " + currentRoom.roomId(),
            () -> roomApi.deleteRoom(currentRoom.roomId(), authToken.token()));
  }

  @Then("the room can be retrieved with the same details")
  public void theRoomCanBeRetrievedWithTheSameDetails() {
    var roomApi = state.useApi(API_TARGET, RoomApi::new);
    var retrieved = roomApi.getRoom(currentRoom.roomId());
    Assertions.assertEquals(currentRoom.roomName(), retrieved.roomName());
    Assertions.assertEquals(currentRoom.type(), retrieved.type());
  }

  public record RoomApiTestData(RoomDataDefaults room) {}
}
