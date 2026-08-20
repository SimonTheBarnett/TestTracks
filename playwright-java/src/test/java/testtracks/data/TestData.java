package testtracks.data;

import java.security.SecureRandom;
import java.util.UUID;

public final class TestData {
  private static final SecureRandom RANDOM = new SecureRandom();

  private TestData() {}

  public static String newScenarioId() {
    return UUID.randomUUID().toString().replace("-", "").substring(0, 8);
  }

  public static int numericSuffix(int minimum, int maximumExclusive) {
    return RANDOM.nextInt(minimum, maximumExclusive);
  }

  public static String safeName(String prefix, String scenarioId, int maxLength) {
    var value = prefix + "_" + scenarioId;
    return value.length() <= maxLength ? value : value.substring(0, maxLength);
  }
}
