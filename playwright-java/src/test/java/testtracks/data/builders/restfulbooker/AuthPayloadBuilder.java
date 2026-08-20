package testtracks.data.builders.restfulbooker;

import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;
import testtracks.configuration.CredentialTarget;

public final class AuthPayloadBuilder {
  private AuthPayloadBuilder() {}

  public static ObjectNode fromCredential(CredentialTarget credential) {
    var payload = JsonNodeFactory.instance.objectNode();
    payload.put("username", credential.username());
    payload.put("password", credential.password());
    return payload;
  }
}
