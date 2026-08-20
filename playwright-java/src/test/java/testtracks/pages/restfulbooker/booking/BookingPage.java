package testtracks.pages.restfulbooker.booking;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.microsoft.playwright.Locator;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.options.AriaRole;
import java.util.List;
import java.util.regex.Pattern;
import testtracks.configuration.TestSettings;
import testtracks.data.builders.restfulbooker.BookingFormData;
import testtracks.ui.common.BasePage;

public final class BookingPage extends BasePage {
  private static final String RESERVE_NOW_BUTTON_NAME = "Reserve Now";
  private static final String BOOKING_API_PATH = "/api/booking";
  private static final Pattern BOOKING_CONFIRMATION_TEXT =
      Pattern.compile("Booking (Successful|Confirmed)", Pattern.CASE_INSENSITIVE);
  private static final String FIRST_NAME_LABEL = "Firstname";
  private static final String FIRST_NAME_LABEL_WITH_SPACE = "First name";
  private static final String FIRST_NAME_LABEL_WITH_CAPITAL = "First Name";
  private static final String LAST_NAME_LABEL = "Lastname";
  private static final String LAST_NAME_LABEL_WITH_SPACE = "Last name";
  private static final String LAST_NAME_LABEL_WITH_CAPITAL = "Last Name";
  private static final String EMAIL_LABEL = "Email";
  private static final String PHONE_LABEL = "Phone";
  private static final ObjectMapper JSON = new ObjectMapper();

  public Locator confirmation() {
    return page.getByText(BOOKING_CONFIRMATION_TEXT);
  }

  private Locator reserveNowButton() {
    return page.locator("#doReservation");
  }

  private Locator submitBookingButton() {
    return page.getByRole(
        AriaRole.BUTTON,
        new Page.GetByRoleOptions().setName(RESERVE_NOW_BUTTON_NAME).setExact(true));
  }

  private Locator fieldByLabel(String label) {
    return page.getByLabel(label);
  }

  public BookingPage(Page page, TestSettings settings) {
    super(page, settings);
  }

  public int bookRoom(BookingFormData booking) {
    reserveNowButton().click();

    fillByPossibleLabels(
        List.of(FIRST_NAME_LABEL, FIRST_NAME_LABEL_WITH_SPACE, FIRST_NAME_LABEL_WITH_CAPITAL),
        booking.firstName());
    fillByPossibleLabels(
        List.of(LAST_NAME_LABEL, LAST_NAME_LABEL_WITH_SPACE, LAST_NAME_LABEL_WITH_CAPITAL),
        booking.lastName());
    fillByPossibleLabels(List.of(EMAIL_LABEL), booking.email());
    fillByPossibleLabels(List.of(PHONE_LABEL), booking.phone());

    var response =
        page.waitForResponse(
            candidate ->
                "POST".equals(candidate.request().method())
                    && candidate.url().toLowerCase().contains(BOOKING_API_PATH),
            () -> submitBookingButton().click());

    var body = response.text();
    if (!response.ok()) {
      throw new IllegalStateException(
          "UI booking request failed with HTTP " + response.status() + ": " + body);
    }

    try {
      return JSON.readTree(body).get("bookingid").asInt();
    } catch (Exception ex) {
      throw new IllegalStateException("UI booking response did not contain a bookingid.", ex);
    }
  }

  private void fillByPossibleLabels(List<String> labels, String value) {
    for (var label : labels) {
      var locator = fieldByLabel(label);
      if (locator.count() > 0) {
        locator.fill(value);
        return;
      }
    }

    throw new IllegalStateException(
        "Could not find any expected booking form label for value '" + value + "'.");
  }
}
