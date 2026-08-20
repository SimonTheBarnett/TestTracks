package testtracks.steps.restfulbooker.api.auth;

import com.fasterxml.jackson.annotation.JsonProperty;

public record TokenValidation(@JsonProperty("valid") boolean valid) {}
