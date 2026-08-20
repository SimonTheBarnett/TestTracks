package testtracks.configuration;

import java.nio.file.Path;
import java.time.OffsetDateTime;
import java.time.ZoneOffset;
import java.time.format.DateTimeFormatter;

public final class RunArtifacts {
  private static final DateTimeFormatter FOLDER_FORMAT =
      DateTimeFormatter.ofPattern("yyyyMMdd-HHmmss");
  private static final Path ROOT_DIRECTORY = Path.of("TestResults");
  private static final Path RUN_DIRECTORY =
      ROOT_DIRECTORY.resolve(createFolderName(OffsetDateTime.now(ZoneOffset.UTC)));

  private RunArtifacts() {}

  public static String directory() {
    return RUN_DIRECTORY.toString();
  }

  public static Path rootDirectory() {
    return ROOT_DIRECTORY;
  }

  public static Path reportHtmlPath() {
    return RUN_DIRECTORY.resolve("test-report.html");
  }

  public static Path cucumberMessagesPath() {
    return RUN_DIRECTORY.resolve("cucumber-messages.ndjson");
  }

  static String createFolderName(OffsetDateTime timestamp) {
    return timestamp.withOffsetSameInstant(ZoneOffset.UTC).format(FOLDER_FORMAT);
  }
}
