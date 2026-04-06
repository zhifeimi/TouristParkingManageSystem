using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Application.EdgeSync;
using TPMS.Domain.Enums;
using TPMS.Infrastructure.Persistence;

namespace TPMS.Infrastructure.Availability;

public sealed class PermitReadService(TpmsDbContext dbContext) : IPermitReadService
{
    public async Task<PermitValidationResultDto> ValidatePermitAsync(
        Guid parkingLotId,
        string licensePlate,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken)
    {
        var normalizedPlate = new string(licensePlate.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

        var permit = await dbContext.Permits
            .AsNoTracking()
            .Where(entity =>
                entity.ParkingLotId == parkingLotId &&
                entity.LicensePlate.Value == normalizedPlate &&
                entity.ValidFromUtc <= observedAtUtc &&
                observedAtUtc <= entity.ValidToUtc)
            .OrderByDescending(entity => entity.ValidToUtc)
            .Select(entity => new
            {
                entity.PermitCode,
                BayNumber = entity.BayNumber.Value,
                entity.ValidToUtc,
                entity.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (permit is null)
        {
            return new PermitValidationResultDto(
                normalizedPlate,
                false,
                null,
                null,
                null,
                PermitStatus.Pending.ToString(),
                "No active permit was found for the license plate.");
        }

        var isValid = permit.Status == PermitStatus.Active;

        return new PermitValidationResultDto(
            normalizedPlate,
            isValid,
            permit.PermitCode,
            permit.BayNumber,
            permit.ValidToUtc,
            permit.Status.ToString(),
            isValid ? "Permit is valid for entry." : "Permit exists but is not currently valid.");
    }
}
