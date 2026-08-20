package testtracks.steps.restfulbooker.api.room;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

public record Rooms(@JsonProperty("rooms") List<Room> items) {}
