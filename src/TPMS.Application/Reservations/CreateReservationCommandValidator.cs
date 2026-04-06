using FluentValidation;

namespace TPMS.Application.Reservations;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(request => request.LotId).NotEmpty();
        RuleFor(request => request.BayId).NotEmpty();
        RuleFor(request => request.TouristName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.TouristEmail).NotEmpty().EmailAddress();
        RuleFor(request => request.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(request => request.EndUtc).GreaterThan(request => request.StartUtc);
    }
}
