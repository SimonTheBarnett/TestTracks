package testtracks.data.builders.restfulbooker;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import testtracks.data.JsonPayloads;
import testtracks.data.TestData;

public final class RoomDataBuilder {
  private static final ObjectMapper JSON = new ObjectMapper();
  private final ObjectNode payload;

  public RoomDataBuilder(RoomDataDefaults defaults) {
    payload = defaults.payload().deepCopy();
  }

  public static RoomDataBuilder forScenario(String scenarioId, RoomDataDefaults defaults) {
    var roomNumber =
        TestData.numericSuffix(defaults.roomIdMinimum(), defaults.roomIdMaximumExclusive());
    var builder = new RoomDataBuilder(defaults);
    builder
        .withRoomId(roomNumber)
        .with("description", builder.stringValue("description") + " " + scenarioId);

    if (!builder.hasValue("roomPrice")) {
      builder.with(
          "roomPrice",
          TestData.numericSuffix(defaults.priceMinimum(), defaults.priceMaximumExclusive()));
    }

    return builder;
  }

  public RoomDataBuilder withRoomId(int roomId) {
    return with("roomid", roomId).with("roomName", Integer.toString(roomId));
  }

  public RoomDataBuilder withType(String type) {
    return with("type", type);
  }

  public RoomDataBuilder with(String propertyName, Object value) {
    payload.set(propertyName, valueToJson(value));
    return this;
  }

  public ObjectNode buildPayload() {
    return payload.deepCopy();
  }

  private String stringValue(String propertyName) {
    return JsonPayloads.stringValue(payload, propertyName);
  }

  private boolean hasValue(String propertyName) {
    return payload.has(propertyName) && !payload.get(propertyName).isNull();
  }

  private static JsonNode valueToJson(Object value) {
    return JSON.valueToTree(value);
  }
}
