using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Playwright;
using TestTracks.Playwright.CSharp.API.Common;
using TestTracks.Playwright.CSharp.Configuration;
using TestTracks.Playwright.CSharp.Diagnostics;

namespace TestTracks.Playwright.CSharp.Specs.Steps.RestfulBooker.API.Booking;

public sealed class BookingApi : BaseApi
{
    public BookingApi(TestSettings settings, IAPIRequestContext request, ApiEvidence? evidence = null)
        : base(settings, request, evidence)
    {
    }

    public async Task<CreatedBooking> CreateBooking(
        JsonObject payload,
        string? token = null,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<CreatedBooking>("POST", "booking", payload, HeadersWithToken(token, headers), query);
    }

    public async Task<Booking> GetBooking(
        int bookingId,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<Booking>("GET", $"booking/{bookingId}", headers: HeadersWithToken(token, headers), query: query);
    }

    public async Task<Bookings> GetBookings(
        string token,
        int? roomId = null,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return await SendJsonAsync<Bookings>("GET", "booking", headers: HeadersWithToken(token, headers), query: QueryWithRoomId(roomId, query));
    }

    public async Task DeleteBooking(
        int bookingId,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        await EnsureOkAsync("DELETE", $"booking/{bookingId}", headers: HeadersWithToken(token, headers), query: query);
    }

    private static IReadOnlyDictionary<string, string>? HeadersWithToken(
        string? token,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var merged = headers is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(token))
        {
            merged["Cookie"] = $"token={token}";
        }

        return merged.Count == 0 ? null : merged;
    }

    private static IReadOnlyDictionary<string, string?>? QueryWithRoomId(
        int? roomId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        var merged = query is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(query, StringComparer.OrdinalIgnoreCase);

        if (roomId is not null)
        {
            merged["roomid"] = roomId.Value.ToString(CultureInfo.InvariantCulture);
        }

        return merged.Count == 0 ? null : merged;
    }
}

public sealed record Booking(
    [property: JsonPropertyName("bookingid")] int BookingId,
    [property: JsonPropertyName("roomid")] int RoomId,
    [property: JsonPropertyName("firstname")] string FirstName,
    [property: JsonPropertyName("lastname")] string LastName,
    [property: JsonPropertyName("depositpaid")] bool DepositPaid,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("bookingdates")] BookingDates BookingDates);

public sealed record BookingDates(
    [property: JsonPropertyName("checkin")] DateOnly CheckIn,
    [property: JsonPropertyName("checkout")] DateOnly CheckOut);

public sealed record Bookings(
    [property: JsonPropertyName("bookings")] IReadOnlyList<Booking> Items);

public sealed record CreatedBooking(
    [property: JsonPropertyName("bookingid")] int BookingId,
    [property: JsonPropertyName("booking")] Booking Booking);
