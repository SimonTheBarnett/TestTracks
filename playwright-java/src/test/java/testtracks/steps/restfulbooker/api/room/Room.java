package testtracks.steps.restfulbooker.api.room;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

public record Room(
    @JsonProperty("roomid") int roomId,
    @JsonProperty("roomName") String roomName,
    @JsonProperty("type") String type,
    @JsonProperty("accessible") boolean accessible,
    @JsonProperty("image") String image,
    @JsonProperty("description") String description,
    @JsonProperty("features") List<String> features,
    @JsonProperty("roomPrice") int roomPrice) {}
