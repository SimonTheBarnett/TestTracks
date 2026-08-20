package testtracks.data.builders.restfulbooker;

import com.fasterxml.jackson.databind.node.ObjectNode;

public record BookingDataDefaults(
    ObjectNode payload,
    int checkInDaysFromToday,
    int checkOutDaysFromToday,
    int firstNameMaxLength,
    int lastNameMaxLength) {
  public BookingDataDefaults {
    firstNameMaxLength = firstNameMaxLength == 0 ? 18 : firstNameMaxLength;
    lastNameMaxLength = lastNameMaxLength == 0 ? 30 : lastNameMaxLength;
  }
}
