using TPMS.Application.Common;

namespace TPMS.Application.Enforcement;

public sealed record RaiseViolationCommand(
    Guid LotId,
    Guid? BayId,
    string? BayNumber,
    string LicensePlate,
    string Reason,
    string Details)
    : ICommand<Result<ViolationDto>>;
