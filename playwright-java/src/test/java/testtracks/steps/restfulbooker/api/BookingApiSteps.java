package testtracks.steps.restfulbooker.api;

import com.fasterxml.jackson.databind.node.ObjectNode;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import org.junit.jupiter.api.Assertions;
import testtracks.data.JsonPayloads;
import testtracks.data.builders.restfulbooker.AuthPayloadBuilder;
import testtracks.data.builders.restfulbooker.BookingDataBuilder;
import testtracks.data.builders.restfulbooker.BookingDataDefaults;
import testtracks.data.builders.restfulbooker.RoomDataBuilder;
import testtracks.data.builders.restfulbooker.RoomDataDefaults;
import testtracks.steps.restfulbooker.api.auth.AuthApi;
import testtracks.steps.restfulbooker.api.auth.AuthToken;
import testtracks.steps.restfulbooker.api.booking.BookingApi;
import testtracks.steps.restfulbooker.api.booking.CreatedBooking;
import testtracks.steps.restfulbooker.api.room.RoomApi;
import testtracks.support.ScenarioState;

public final class BookingApiSteps {
  private static final String API_TARGET = "restfulBookerApi";
  private static final String ADMIN_CREDENTIAL = "restfulBookerAdmin";

  private final ScenarioState state;
  private AuthToken authToken;
  private ObjectNode bookingPayload;
  private CreatedBooking createdBooking;

  public BookingApiSteps(ScenarioState state) {
    this.state = state;
  }

  @Given("valid booking details")
  public void validBookingDetails() {
    var data =
        state.data().load("api-booking", "scenarios.validBookingDetails", BookingApiTestData.class);
    var authApi = state.useApi(API_TARGET, AuthApi::new);
    var roomApi = state.useApi(API_TARGET, RoomApi::new);
    var admin = state.targets().credential(ADMIN_CREDENTIAL);

    authToken = authApi.logIn(AuthPayloadBuilder.fromCredential(admin));

    var room = RoomDataBuilder.forScenario(state.scenarioId(), data.room()).buildPayload();
    var createdRoom = roomApi.createRoom(room, authToken.token());
    state
        .cleanup()
        .register(
            "Delete room " + createdRoom.roomId(),
            () -> roomApi.deleteRoom(createdRoom.roomId(), authToken.token()));

    bookingPayload =
        BookingDataBuilder.forScenario(state.scenarioId(), data.booking())
            .forRoom(createdRoom.roomId())
            .buildPayload();
  }

  @When("the booking is created through the booking API")
  public void theBookingIsCreatedThroughTheBookingApi() {
    var bookingApi = state.useApi(API_TARGET, BookingApi::new);
    createdBooking = bookingApi.createBooking(bookingPayload, authToken.token());

    state
        .cleanup()
        .register(
            "Delete booking " + createdBooking.bookingId(),
            () -> bookingApi.deleteBooking(createdBooking.bookingId(), authToken.token()));
  }

  @Then("the booking can be retrieved with the same details")
  public void theBookingCanBeRetrievedWithTheSameDetails() {
    var bookingApi = state.useApi(API_TARGET, BookingApi::new);
    var retrieved = bookingApi.getBooking(createdBooking.bookingId(), authToken.token());

    Assertions.assertEquals(
        JsonPayloads.stringValue(bookingPayload, "firstname"), retrieved.firstName());
    Assertions.assertEquals(
        JsonPayloads.stringValue(bookingPayload, "lastname"), retrieved.lastName());
    Assertions.assertEquals(JsonPayloads.intValue(bookingPayload, "roomid"), retrieved.roomId());
  }

  public record BookingApiTestData(BookingDataDefaults booking, RoomDataDefaults room) {}
}
