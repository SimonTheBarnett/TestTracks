package testtracks.steps.restfulbooker.api.booking;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

public record Bookings(@JsonProperty("bookings") List<Booking> items) {}
