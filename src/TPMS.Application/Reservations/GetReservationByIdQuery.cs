using TPMS.Application.Common;

namespace TPMS.Application.Reservations;

public sealed record GetReservationByIdQuery(Guid ReservationId) : IQuery<Result<ReservationDto>>;
