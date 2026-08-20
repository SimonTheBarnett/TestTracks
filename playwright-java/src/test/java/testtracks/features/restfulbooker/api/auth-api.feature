@api @auth @smoke
Feature: Auth API

  Scenario: Valid admin credentials produce a token
    When valid admin credentials are submitted to the auth API
    Then a reusable auth token is returned

  Scenario: Invalid admin credentials are rejected
    When invalid admin credentials are submitted to the auth API
    Then the credentials are rejected
