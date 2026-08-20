package testtracks.data;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.node.ObjectNode;

public final class JsonPayloads {
  private JsonPayloads() {}

  public static String stringValue(ObjectNode payload, String propertyName) {
    var value = payload.get(propertyName);
    if (value == null || value.isNull()) {
      throw new IllegalStateException("Payload is missing '" + propertyName + "'.");
    }
    return value.asText();
  }

  public static int intValue(ObjectNode payload, String propertyName) {
    var value = payload.get(propertyName);
    if (value == null || value.isNull()) {
      throw new IllegalStateException("Payload is missing '" + propertyName + "'.");
    }
    return value.asInt();
  }

  public static String stringValue(JsonNode payload, String propertyName) {
    var value = payload.get(propertyName);
    if (value == null || value.isNull()) {
      throw new IllegalStateException("Payload is missing '" + propertyName + "'.");
    }
    return value.asText();
  }
}
