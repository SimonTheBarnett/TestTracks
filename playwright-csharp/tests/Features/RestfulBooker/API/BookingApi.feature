@api @booking
Feature: Booking API

  Scenario: A booking can be created and retrieved
    Given valid booking details
    When the booking is created through the booking API
    Then the booking can be retrieved with the same details
