package testtracks.data.builders.restfulbooker;

import com.fasterxml.jackson.databind.node.ObjectNode;

public record RoomDataDefaults(
    ObjectNode payload,
    int priceMinimum,
    int priceMaximumExclusive,
    int roomIdMinimum,
    int roomIdMaximumExclusive) {
  public RoomDataDefaults {
    priceMinimum = priceMinimum == 0 ? 300 : priceMinimum;
    priceMaximumExclusive = priceMaximumExclusive == 0 ? 900 : priceMaximumExclusive;
    roomIdMinimum = roomIdMinimum == 0 ? 7000 : roomIdMinimum;
    roomIdMaximumExclusive = roomIdMaximumExclusive == 0 ? 9999 : roomIdMaximumExclusive;
  }
}
