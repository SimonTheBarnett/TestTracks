package testtracks.steps.restfulbooker.api.auth;

import com.fasterxml.jackson.annotation.JsonProperty;

public record AuthToken(@JsonProperty("token") String token) {}
