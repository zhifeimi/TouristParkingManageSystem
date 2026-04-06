using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Enforcement;

public sealed class RaiseViolationCommandHandler(
    IViolationCaseRepository violationCaseRepository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<RaiseViolationCommand, Result<ViolationDto>>
{
    public async Task<Result<ViolationDto>> Handle(RaiseViolationCommand request, CancellationToken cancellationToken)
    {
        var violation = new ViolationCase(
            Guid.NewGuid(),
            request.LotId,
            request.BayId,
            string.IsNullOrWhiteSpace(request.BayNumber) ? null : new BayNumber(request.BayNumber),
            new LicensePlate(request.LicensePlate),
            request.Reason,
            request.Details,
            dateTimeProvider.UtcNow);

        await violationCaseRepository.AddAsync(violation, cancellationToken);

        return Result<ViolationDto>.Success(new ViolationDto(
            violation.Id,
            violation.ParkingLotId,
            violation.ParkingBayId,
            violation.BayNumber?.Value,
            violation.LicensePlate.Value,
            violation.Reason,
            violation.Details,
            violation.Status.ToString(),
            violation.CreatedAtUtc));
    }
}
