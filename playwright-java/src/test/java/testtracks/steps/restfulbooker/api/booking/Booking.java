package testtracks.steps.restfulbooker.api.booking;

import com.fasterxml.jackson.annotation.JsonProperty;

public record Booking(
    @JsonProperty("bookingid") int bookingId,
    @JsonProperty("roomid") int roomId,
    @JsonProperty("firstname") String firstName,
    @JsonProperty("lastname") String lastName,
    @JsonProperty("depositpaid") boolean depositPaid,
    @JsonProperty("email") String email,
    @JsonProperty("phone") String phone,
    @JsonProperty("bookingdates") BookingDates bookingDates) {}
