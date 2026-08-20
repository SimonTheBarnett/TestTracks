package testtracks.data.builders.restfulbooker;

public record BookingFormData(
    int bookingId,
    int roomId,
    String firstName,
    String lastName,
    boolean depositPaid,
    String email,
    String phone,
    BookingDateRange bookingDates) {}
