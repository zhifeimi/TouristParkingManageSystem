using TPMS.Application.Common;

namespace TPMS.Application.Reservations;

public sealed record ReassignBayCommand(
    Guid ReservationId,
    Guid? TargetBayId,
    string Reason,
    string? Note)
    : ICommand<Result<ReservationDto>>;
