package testtracks.steps.restfulbooker.api.booking;

import com.fasterxml.jackson.annotation.JsonProperty;

public record BookingDates(
    @JsonProperty("checkin") String checkIn, @JsonProperty("checkout") String checkOut) {}
