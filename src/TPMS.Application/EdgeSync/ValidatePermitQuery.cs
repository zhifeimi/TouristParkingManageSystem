using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed record ValidatePermitQuery(Guid ParkingLotId, string LicensePlate, DateTimeOffset ObservedAtUtc)
    : IQuery<Result<PermitValidationResultDto>>;
