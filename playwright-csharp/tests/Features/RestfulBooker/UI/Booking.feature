@ui @booking
Feature: Guest booking

  Scenario: A guest can create a booking for an available room
    Given an available room exists
    When the guest books the room
    Then the booking is shown as confirmed

  @admin
  Scenario: An administrator can see an API-created room
    Given an available room exists
    When an administrator views the rooms
    Then the room is visible to the administrator
