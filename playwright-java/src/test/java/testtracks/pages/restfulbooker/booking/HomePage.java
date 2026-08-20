package testtracks.pages.restfulbooker.booking;

import com.microsoft.playwright.Page;
import java.time.format.DateTimeFormatter;
import testtracks.configuration.TestSettings;
import testtracks.data.builders.restfulbooker.BookingDateRange;
import testtracks.ui.common.BasePage;

public final class HomePage extends BasePage {
  private static final DateTimeFormatter DATE_FORMAT = DateTimeFormatter.ISO_LOCAL_DATE;

  public HomePage(Page page, TestSettings settings) {
    super(page, settings);
  }

  public void openBookingForRoom(int roomId, BookingDateRange bookingDates) {
    var checkIn = bookingDates.checkIn().format(DATE_FORMAT);
    var checkOut = bookingDates.checkOut().format(DATE_FORMAT);

    open("reservation/" + roomId + "?checkin=" + checkIn + "&checkout=" + checkOut);
  }
}
