package testtracks.steps.restfulbooker.ui;

import static com.microsoft.playwright.assertions.PlaywrightAssertions.assertThat;

import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import org.junit.jupiter.api.Assertions;
import testtracks.data.builders.restfulbooker.AuthPayloadBuilder;
import testtracks.data.builders.restfulbooker.BookingDataBuilder;
import testtracks.data.builders.restfulbooker.BookingDataDefaults;
import testtracks.data.builders.restfulbooker.RoomDataBuilder;
import testtracks.data.builders.restfulbooker.RoomDataDefaults;
import testtracks.pages.restfulbooker.admin.AdminLoginPage;
import testtracks.pages.restfulbooker.admin.RoomsPage;
import testtracks.pages.restfulbooker.booking.BookingPage;
import testtracks.pages.restfulbooker.booking.HomePage;
import testtracks.steps.restfulbooker.api.auth.AuthApi;
import testtracks.steps.restfulbooker.api.auth.AuthToken;
import testtracks.steps.restfulbooker.api.booking.BookingApi;
import testtracks.steps.restfulbooker.api.room.RoomApi;
import testtracks.support.ScenarioState;

public final class BookingSteps {
  private static final String SITE_TARGET = "restfulBooker";
  private static final String API_TARGET = "restfulBookerApi";
  private static final String ADMIN_CREDENTIAL = "restfulBookerAdmin";

  private final ScenarioState state;
  private BookingPage bookingPage;
  private RoomsPage roomsPage;
  private AuthToken authToken;
  private Integer currentRoomId;
  private String currentRoomName;
  private Integer createdBookingId;

  public BookingSteps(ScenarioState state) {
    this.state = state;
  }

  @Given("an available room exists")
  public void anAvailableRoomExists() {
    var data =
        state.data().load("ui-booking", "scenarios.availableRoom", AvailableRoomUiTestData.class);
    var authApi = state.useApi(API_TARGET, AuthApi::new);
    var roomApi = state.useApi(API_TARGET, RoomApi::new);
    var admin = state.targets().credential(ADMIN_CREDENTIAL);

    authToken = authApi.logIn(AuthPayloadBuilder.fromCredential(admin));

    var room = RoomDataBuilder.forScenario(state.scenarioId(), data.room()).buildPayload();
    var createdRoom = roomApi.createRoom(room, authToken.token());

    currentRoomId = createdRoom.roomId();
    currentRoomName = createdRoom.roomName();
    state
        .cleanup()
        .register(
            "Delete room " + createdRoom.roomId(),
            () -> roomApi.deleteRoom(createdRoom.roomId(), authToken.token()));
  }

  @When("the guest books the room")
  public void theGuestBooksTheRoom() {
    var data =
        state
            .data()
            .load("ui-booking", "scenarios.guestCanCreateBooking", GuestBookingUiTestData.class);
    var page = state.usePage(SITE_TARGET);

    var homePage = new HomePage(page, state.settings());
    bookingPage = new BookingPage(page, state.settings());
    var bookingApi = state.useApi(API_TARGET, BookingApi::new);

    var bookingDetails =
        BookingDataBuilder.forScenario(state.scenarioId(), data.booking())
            .forRoom(currentRoomId)
            .buildFormData();

    homePage.openBookingForRoom(currentRoomId, bookingDetails.bookingDates());
    createdBookingId = bookingPage.bookRoom(bookingDetails);

    state
        .cleanup()
        .register(
            "Delete booking " + createdBookingId,
            () -> bookingApi.deleteBooking(createdBookingId, authToken.token()));
  }

  @Then("the booking is shown as confirmed")
  public void theBookingIsShownAsConfirmed() {
    assertThat(bookingPage.confirmation())
        .isVisible(
            new com.microsoft.playwright.assertions.LocatorAssertions.IsVisibleOptions()
                .setTimeout(state.settings().expectTimeoutMs()));
    Assertions.assertTrue(createdBookingId > 0);
  }

  @When("an administrator views the rooms")
  public void anAdministratorViewsTheRooms() {
    var admin = state.targets().credential(ADMIN_CREDENTIAL);
    var page = state.usePage(SITE_TARGET);

    var adminLoginPage = new AdminLoginPage(page, state.settings());
    roomsPage = new RoomsPage(page, state.settings());
    adminLoginPage.logIn(admin.username(), admin.password());
  }

  @Then("the room is visible to the administrator")
  public void theRoomIsVisibleToTheAdministrator() {
    assertThat(roomsPage.roomNamed(currentRoomName))
        .isVisible(
            new com.microsoft.playwright.assertions.LocatorAssertions.IsVisibleOptions()
                .setTimeout(state.settings().expectTimeoutMs()));
  }

  public record AvailableRoomUiTestData(RoomDataDefaults room) {}

  public record GuestBookingUiTestData(BookingDataDefaults booking) {}
}
