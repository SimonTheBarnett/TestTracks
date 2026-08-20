package testtracks.steps.restfulbooker.api.room;

import com.fasterxml.jackson.databind.node.ObjectNode;
import com.microsoft.playwright.APIRequestContext;
import java.util.HashMap;
import java.util.Map;
import testtracks.api.common.BaseApi;
import testtracks.configuration.TestSettings;
import testtracks.data.JsonPayloads;
import testtracks.diagnostics.ApiEvidence;

public final class RoomApi extends BaseApi {
  public RoomApi(TestSettings settings, APIRequestContext request, ApiEvidence evidence) {
    super(settings, request, evidence);
  }

  public Room createRoom(ObjectNode payload, String token) {
    ensureOk("POST", "room", payload, headersWithToken(token, null), null);

    var rooms = getRooms(null, null);
    var roomName = JsonPayloads.stringValue(payload, "roomName");
    return rooms.items().stream()
        .filter(candidate -> candidate.roomName().equals(roomName))
        .reduce((first, second) -> second)
        .orElseThrow(
            () ->
                new IllegalStateException(
                    "Created room '" + roomName + "' was not returned by GET room."));
  }

  public Room getRoom(int roomId) {
    return sendJson("GET", "room/" + roomId, null, null, null, Room.class);
  }

  public Rooms getRooms(Map<String, String> headers, Map<String, String> query) {
    return sendJson("GET", "room", null, headers, query, Rooms.class);
  }

  public void deleteRoom(int roomId, String token) {
    ensureOk("DELETE", "room/" + roomId, null, headersWithToken(token, null), null);
  }

  private static Map<String, String> headersWithToken(String token, Map<String, String> headers) {
    var merged = headers == null ? new HashMap<String, String>() : new HashMap<>(headers);
    merged.put("Cookie", "token=" + token);
    return merged;
  }
}
