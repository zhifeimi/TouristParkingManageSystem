using FluentValidation;

namespace TPMS.Application.Reservations;

public sealed class ReassignBayCommandValidator : AbstractValidator<ReassignBayCommand>
{
    public ReassignBayCommandValidator()
    {
        RuleFor(request => request.ReservationId).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(100);
    }
}
