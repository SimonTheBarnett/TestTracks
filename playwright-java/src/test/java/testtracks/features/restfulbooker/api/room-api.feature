@api @room
Feature: Room API

  Scenario: A room can be created and retrieved
    Given valid room details
    When the room is created through the room API
    Then the room can be retrieved with the same details
