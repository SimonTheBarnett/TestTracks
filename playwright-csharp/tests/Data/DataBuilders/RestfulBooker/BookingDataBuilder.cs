using System.Globalization;
using System.Text.Json.Nodes;
using TestTracks.Playwright.CSharp.Specs.Data;

namespace TestTracks.Playwright.CSharp.Specs.Data.DataBuilders.RestfulBooker;

public sealed record BookingDataDefaults
{
    public required JsonObject Payload { get; init; }

    public int CheckInDaysFromToday { get; init; }

    public int CheckOutDaysFromToday { get; init; }

    public int FirstNameMaxLength { get; init; } = 18;

    public int LastNameMaxLength { get; init; } = 30;
}

public sealed class BookingDataBuilder
{
    private const int NewBookingId = 0;
    private readonly JsonObject _payload;

    public BookingDataBuilder(BookingDataDefaults defaults)
    {
        _payload = defaults.Payload.DeepClone().AsObject();
        SetBookingDates(defaults.CheckInDaysFromToday, defaults.CheckOutDaysFromToday);
    }

    public static BookingDataBuilder ForScenario(string scenarioId, BookingDataDefaults defaults)
    {
        var builder = new BookingDataBuilder(defaults);
        var firstName = builder.StringValue("firstname");
        var lastName = $"{builder.StringValue("lastname")}{scenarioId}";
        var email = builder.StringValue("email");
        var emailParts = email.Split('@', 2);
        var emailPrefix = emailParts[0];
        var emailDomain = emailParts.Length == 2 ? emailParts[1] : "example.test";

        return builder
            .With("firstname", TestData.SafeName(firstName, scenarioId, defaults.FirstNameMaxLength))
            .With("lastname", lastName[..Math.Min(defaults.LastNameMaxLength, lastName.Length)])
            .With("email", $"{emailPrefix}.{scenarioId}@{emailDomain}");
    }

    public BookingDataBuilder ForRoom(int roomId)
    {
        return With("roomid", roomId);
    }

    public BookingDataBuilder With(string propertyName, object? value)
    {
        _payload[propertyName] = JsonValue.Create(value);
        return this;
    }

    public JsonObject BuildPayload()
    {
        return _payload.DeepClone().AsObject();
    }

    public BookingFormData BuildFormData()
    {
        var dates = _payload["bookingdates"]?.AsObject()
            ?? throw new InvalidOperationException("Booking payload is missing 'bookingdates'.");

        return new BookingFormData(
            NewBookingId,
            IntValue("roomid"),
            StringValue("firstname"),
            StringValue("lastname"),
            BoolValue("depositpaid"),
            StringValue("email"),
            StringValue("phone"),
            new BookingDateRange(
                DateOnly.Parse(StringValue(dates, "checkin"), CultureInfo.InvariantCulture),
                DateOnly.Parse(StringValue(dates, "checkout"), CultureInfo.InvariantCulture)));
    }

    private void SetBookingDates(int checkInDaysFromToday, int checkOutDaysFromToday)
    {
        var bookingDates = _payload["bookingdates"] as JsonObject;
        if (bookingDates is null)
        {
            bookingDates = new JsonObject();
            _payload["bookingdates"] = bookingDates;
        }

        bookingDates["checkin"] = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(checkInDaysFromToday))
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        bookingDates["checkout"] = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(checkOutDaysFromToday))
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private string StringValue(string propertyName)
    {
        return StringValue(_payload, propertyName);
    }

    private static string StringValue(JsonObject payload, string propertyName)
    {
        return payload[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"Booking payload is missing '{propertyName}'.");
    }

    private int IntValue(string propertyName)
    {
        return _payload[propertyName]?.GetValue<int>()
            ?? throw new InvalidOperationException($"Booking payload is missing '{propertyName}'.");
    }

    private bool BoolValue(string propertyName)
    {
        return _payload[propertyName]?.GetValue<bool>()
            ?? throw new InvalidOperationException($"Booking payload is missing '{propertyName}'.");
    }
}

public sealed record BookingFormData(
    int BookingId,
    int RoomId,
    string FirstName,
    string LastName,
    bool DepositPaid,
    string Email,
    string Phone,
    BookingDateRange BookingDates);

public sealed record BookingDateRange(DateOnly CheckIn, DateOnly CheckOut);
