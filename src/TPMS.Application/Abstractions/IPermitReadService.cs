using TPMS.Application.EdgeSync;

namespace TPMS.Application.Abstractions;

public interface IPermitReadService
{
    Task<PermitValidationResultDto> ValidatePermitAsync(
        Guid parkingLotId,
        string licensePlate,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken);
}
