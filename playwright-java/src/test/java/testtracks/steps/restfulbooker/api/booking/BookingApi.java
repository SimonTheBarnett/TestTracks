package testtracks.steps.restfulbooker.api.booking;

import com.fasterxml.jackson.databind.node.ObjectNode;
import com.microsoft.playwright.APIRequestContext;
import java.util.HashMap;
import java.util.Map;
import testtracks.api.common.BaseApi;
import testtracks.configuration.TestSettings;
import testtracks.diagnostics.ApiEvidence;

public final class BookingApi extends BaseApi {
  public BookingApi(TestSettings settings, APIRequestContext request, ApiEvidence evidence) {
    super(settings, request, evidence);
  }

  public CreatedBooking createBooking(ObjectNode payload, String token) {
    return sendJson(
        "POST", "booking", payload, headersWithToken(token, null), null, CreatedBooking.class);
  }

  public Booking getBooking(int bookingId, String token) {
    return sendJson(
        "GET", "booking/" + bookingId, null, headersWithToken(token, null), null, Booking.class);
  }

  public Bookings getBookings(String token, Integer roomId) {
    Map<String, String> query = roomId == null ? null : Map.of("roomid", Integer.toString(roomId));
    return sendJson("GET", "booking", null, headersWithToken(token, null), query, Bookings.class);
  }

  public void deleteBooking(int bookingId, String token) {
    ensureOk("DELETE", "booking/" + bookingId, null, headersWithToken(token, null), null);
  }

  private static Map<String, String> headersWithToken(String token, Map<String, String> headers) {
    var merged = headers == null ? new HashMap<String, String>() : new HashMap<>(headers);
    if (token != null && !token.isBlank()) {
      merged.put("Cookie", "token=" + token);
    }
    return merged.isEmpty() ? null : merged;
  }
}
