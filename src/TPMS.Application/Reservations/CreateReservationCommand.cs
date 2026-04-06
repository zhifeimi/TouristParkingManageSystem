using TPMS.Application.Common;

namespace TPMS.Application.Reservations;

public sealed record CreateReservationCommand(
    Guid LotId,
    Guid BayId,
    string TouristName,
    string TouristEmail,
    bool IsGuestReservation,
    string LicensePlate,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string? SuccessUrl,
    string? CancelUrl)
    : ICommand<Result<ReservationDto>>,
      IAvailabilityWindowRequest;
