using FluentValidation;

namespace TPMS.Application.Enforcement;

public sealed class RaiseViolationCommandValidator : AbstractValidator<RaiseViolationCommand>
{
    public RaiseViolationCommandValidator()
    {
        RuleFor(request => request.LotId).NotEmpty();
        RuleFor(request => request.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Details).NotEmpty().MaximumLength(500);
    }
}
