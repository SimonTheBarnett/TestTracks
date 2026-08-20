package testtracks.diagnostics;

import java.util.regex.Pattern;

public final class SecretRedactor {
  private static final String REPLACEMENT = "[REDACTED]";
  private static final String JSON_SECRET_FIELDS =
      "accessToken|apiKey|authorization|clientSecret|cookie|password|refreshToken|secret|sessionId|token";
  private static final String KEY_VALUE_SECRET_NAMES =
      "access_token|api_key|client_secret|password|refresh_token|sessionid|token";
  private static final String HEADER_SECRET_NAMES = "Authorization|Cookie|Set-Cookie|X-Api-Key";
  private static final Pattern JSON_SECRET =
      Pattern.compile(
          "(\"(?:" + JSON_SECRET_FIELDS + ")\"\\s*:\\s*\")([^\"]*)(\")", Pattern.CASE_INSENSITIVE);
  private static final Pattern KEY_VALUE_SECRET =
      Pattern.compile("((?:" + KEY_VALUE_SECRET_NAMES + ")=)[^;\\s&]+", Pattern.CASE_INSENSITIVE);
  private static final Pattern HEADER_SECRET =
      Pattern.compile(
          "((?:" + HEADER_SECRET_NAMES + ")\\s*:\\s*)[^\\r\\n]+", Pattern.CASE_INSENSITIVE);

  private SecretRedactor() {}

  public static String redact(String value) {
    if (value == null || value.isBlank()) {
      return "";
    }

    var redacted = JSON_SECRET.matcher(value).replaceAll("$1" + REPLACEMENT + "$3");
    redacted = KEY_VALUE_SECRET.matcher(redacted).replaceAll("$1" + REPLACEMENT);
    return HEADER_SECRET.matcher(redacted).replaceAll("$1" + REPLACEMENT);
  }
}
