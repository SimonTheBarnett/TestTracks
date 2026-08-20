package testtracks.configuration;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.stream.Collectors;

public final class EnvironmentDataStore {
  private static final Path ENVIRONMENTS_ROOT =
      Path.of("src", "test", "java", "testtracks", "data", "environments");
  private static final ObjectMapper JSON =
      new ObjectMapper()
          .configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true)
          .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

  private final String environmentName;
  private final Path environmentDirectory;

  public EnvironmentDataStore(String environmentName) {
    this.environmentName = validateEnvironmentName(environmentName);
    environmentDirectory = ENVIRONMENTS_ROOT.resolve(this.environmentName);
    if (!Files.isDirectory(environmentDirectory)) {
      throw new IllegalStateException(
          "Environment '"
              + environmentName
              + "' was not found. Available environments: "
              + availableEnvironments()
              + ".");
    }
  }

  public String environmentName() {
    return environmentName;
  }

  public <T> T load(String fileName, Class<T> type) {
    return load(fileName, null, type);
  }

  public <T> T load(String fileName, String sectionPath, Class<T> type) {
    var path = environmentDirectory.resolve(fileName + ".json");
    if (!Files.exists(path)) {
      throw new IllegalStateException(
          "Missing environment test data file: " + path.toAbsolutePath().normalize());
    }

    try (var stream = Files.newInputStream(path)) {
      var root = JSON.readTree(stream);
      var selected = selectSection(root, sectionPath, path);
      return JSON.treeToValue(selected, type);
    } catch (IOException ex) {
      throw new IllegalStateException("Environment test data was empty or invalid: " + path, ex);
    }
  }

  private static JsonNode selectSection(JsonNode root, String sectionPath, Path path) {
    if (sectionPath == null || sectionPath.isBlank()) {
      return root;
    }

    var current = root;
    for (var segment : sectionPath.split("\\.")) {
      current = current.get(segment.trim());
      if (current == null) {
        throw new IllegalStateException(
            "Section '" + sectionPath + "' was not found in environment test data file: " + path);
      }
    }

    return current;
  }

  private static String validateEnvironmentName(String value) {
    if (value == null
        || value.isBlank()
        || value.contains("/")
        || value.contains("\\")
        || value.contains("..")) {
      throw new IllegalStateException(
          "Invalid ENV value '"
              + value
              + "'. Use a folder name under src/test/java/testtracks/data/environments.");
    }
    return value.trim();
  }

  private static String availableEnvironments() {
    if (!Files.isDirectory(ENVIRONMENTS_ROOT)) {
      return "none";
    }

    try (var directories = Files.list(ENVIRONMENTS_ROOT)) {
      var values =
          directories
              .filter(Files::isDirectory)
              .map(path -> path.getFileName().toString())
              .sorted()
              .collect(Collectors.joining(", "));
      return values.isBlank() ? "none" : values;
    } catch (IOException ex) {
      return "unavailable";
    }
  }
}
