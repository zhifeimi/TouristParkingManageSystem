using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.Reservations;

public sealed class GetReservationByIdQueryHandler(IAvailabilityReadService availabilityReadService)
    : IRequestHandler<GetReservationByIdQuery, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await availabilityReadService.GetReservationAsync(request.ReservationId, cancellationToken);
        return reservation is null
            ? Result<ReservationDto>.NotFound("Reservation was not found.")
            : Result<ReservationDto>.Success(reservation);
    }
}
