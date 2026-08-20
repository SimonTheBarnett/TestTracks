package testtracks.data.builders.restfulbooker;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;
import java.time.LocalDate;
import testtracks.data.JsonPayloads;
import testtracks.data.TestData;

public final class BookingDataBuilder {
  private static final int NEW_BOOKING_ID = 0;
  private static final ObjectMapper JSON = new ObjectMapper();
  private final ObjectNode payload;

  public BookingDataBuilder(BookingDataDefaults defaults) {
    payload = defaults.payload().deepCopy();
    setBookingDates(defaults.checkInDaysFromToday(), defaults.checkOutDaysFromToday());
  }

  public static BookingDataBuilder forScenario(String scenarioId, BookingDataDefaults defaults) {
    var builder = new BookingDataBuilder(defaults);
    var firstName = builder.stringValue("firstname");
    var lastName = builder.stringValue("lastname") + scenarioId;
    var email = builder.stringValue("email");
    var emailParts = email.split("@", 2);
    var emailPrefix = emailParts[0];
    var emailDomain = emailParts.length == 2 ? emailParts[1] : "example.test";

    return builder
        .with("firstname", TestData.safeName(firstName, scenarioId, defaults.firstNameMaxLength()))
        .with(
            "lastname",
            lastName.substring(0, Math.min(defaults.lastNameMaxLength(), lastName.length())))
        .with("email", emailPrefix + "." + scenarioId + "@" + emailDomain);
  }

  public BookingDataBuilder forRoom(int roomId) {
    return with("roomid", roomId);
  }

  public BookingDataBuilder with(String propertyName, Object value) {
    payload.set(propertyName, valueToJson(value));
    return this;
  }

  public ObjectNode buildPayload() {
    return payload.deepCopy();
  }

  public BookingFormData buildFormData() {
    var dates = payload.get("bookingdates");
    if (!(dates instanceof ObjectNode bookingDates)) {
      throw new IllegalStateException("Booking payload is missing 'bookingdates'.");
    }

    return new BookingFormData(
        NEW_BOOKING_ID,
        intValue("roomid"),
        stringValue("firstname"),
        stringValue("lastname"),
        boolValue("depositpaid"),
        stringValue("email"),
        stringValue("phone"),
        new BookingDateRange(
            LocalDate.parse(JsonPayloads.stringValue(bookingDates, "checkin")),
            LocalDate.parse(JsonPayloads.stringValue(bookingDates, "checkout"))));
  }

  private void setBookingDates(int checkInDaysFromToday, int checkOutDaysFromToday) {
    ObjectNode bookingDates;
    if (payload.get("bookingdates") instanceof ObjectNode existing) {
      bookingDates = existing;
    } else {
      bookingDates = JsonNodeFactory.instance.objectNode();
      payload.set("bookingdates", bookingDates);
    }

    bookingDates.put("checkin", LocalDate.now().plusDays(checkInDaysFromToday).toString());
    bookingDates.put("checkout", LocalDate.now().plusDays(checkOutDaysFromToday).toString());
  }

  private String stringValue(String propertyName) {
    return JsonPayloads.stringValue(payload, propertyName);
  }

  private int intValue(String propertyName) {
    return JsonPayloads.intValue(payload, propertyName);
  }

  private boolean boolValue(String propertyName) {
    var value = payload.get(propertyName);
    if (value == null || value.isNull()) {
      throw new IllegalStateException("Booking payload is missing '" + propertyName + "'.");
    }
    return value.asBoolean();
  }

  private static JsonNode valueToJson(Object value) {
    return JSON.valueToTree(value);
  }
}
