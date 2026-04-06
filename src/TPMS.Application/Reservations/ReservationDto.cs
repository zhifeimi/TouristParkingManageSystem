namespace TPMS.Application.Reservations;

public sealed record ReservationDto(
    Guid ReservationId,
    Guid ParkingLotId,
    Guid ParkingBayId,
    string BayNumber,
    string Status,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string LicensePlate,
    decimal TotalAmount,
    string Currency,
    bool NeedsResolution,
    string? ResolutionNote,
    CreatePaymentSessionResponse? PaymentSession);
