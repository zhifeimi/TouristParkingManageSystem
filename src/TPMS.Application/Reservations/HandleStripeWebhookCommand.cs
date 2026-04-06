using TPMS.Application.Common;

namespace TPMS.Application.Reservations;

public sealed record HandleStripeWebhookCommand(string Payload, string SignatureHeader)
    : ICommand<Result<ReservationDto>>;
