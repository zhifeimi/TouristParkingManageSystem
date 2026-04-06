using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed class ValidatePermitQueryHandler(IPermitReadService permitReadService)
    : IRequestHandler<ValidatePermitQuery, Result<PermitValidationResultDto>>
{
    public async Task<Result<PermitValidationResultDto>> Handle(ValidatePermitQuery request, CancellationToken cancellationToken)
    {
        var validation = await permitReadService.ValidatePermitAsync(
            request.ParkingLotId,
            request.LicensePlate,
            request.ObservedAtUtc,
            cancellationToken);

        return Result<PermitValidationResultDto>.Success(validation);
    }
}
