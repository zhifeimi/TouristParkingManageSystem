using FluentAssertions;
using Moq;
using TPMS.Application.Abstractions;
using TPMS.Application.Availability;
using TPMS.Application.Lots;
using TPMS.Application.Reservations;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Tests.Reservations;

public sealed class ReservationHandlersTests
{
    [Fact]
    public async Task CreateReservation_should_return_checkout_session_for_available_bay()
    {
        var now = DateTimeOffset.UtcNow;
        var lot = new ParkingLot(Guid.NewGuid(), "LOT-1", "Main Lot", "Australia/Sydney", new Money(12.5m, "AUD"));
        var bay = new ParkingBay(Guid.NewGuid(), lot.Id, new BayNumber("A1"), BayType.Standard);

        var lotRepository = new Mock<IParkingLotRepository>();
        var bayRepository = new Mock<IParkingBayRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var paymentRepository = new Mock<IPaymentRepository>();
        var availabilityReadService = new Mock<IAvailabilityReadService>();
        var paymentGateway = new Mock<IPaymentGateway>();
        var clock = new Mock<IDateTimeProvider>();

        lotRepository.Setup(repository => repository.GetByIdAsync(lot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lot);
        bayRepository.Setup(repository => repository.GetByIdAsync(bay.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bay);
        reservationRepository.Setup(repository => repository.HasOverlappingReservationAsync(bay.Id, It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        availabilityReadService
            .Setup(service => service.GetLotAvailabilityAsync(lot.Id, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LotAvailabilitySummaryDto(
                lot.Id,
                lot.Name,
                now.AddHours(1),
                now.AddHours(3),
                1,
                1,
                0,
                0,
                [new BayAvailabilityDto(bay.Id, bay.BayNumber.Value, bay.BayType.ToString(), true, false, false, false, null)]));
        paymentGateway
            .Setup(gateway => gateway.CreateCheckoutSessionAsync(It.IsAny<PaymentCheckoutRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentCheckoutSession("sess_123", "https://checkout.example/sess_123"));
        clock.SetupGet(provider => provider.UtcNow).Returns(now);

        var handler = new CreateReservationCommandHandler(
            lotRepository.Object,
            bayRepository.Object,
            reservationRepository.Object,
            paymentRepository.Object,
            availabilityReadService.Object,
            paymentGateway.Object,
            clock.Object);

        var result = await handler.Handle(new CreateReservationCommand(
            lot.Id,
            bay.Id,
            "Guest Tourist",
            "guest@example.com",
            true,
            "ABC123",
            now.AddHours(1),
            now.AddHours(3),
            "https://example.com/success",
            "https://example.com/cancel"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BayNumber.Should().Be("A1");
        result.Value.PaymentSession!.SessionId.Should().Be("sess_123");
        reservationRepository.Verify(repository => repository.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        paymentRepository.Verify(repository => repository.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReassignBay_should_mark_reservation_for_resolution_when_no_target_exists()
    {
        var now = DateTimeOffset.UtcNow;
        var lot = new ParkingLot(Guid.NewGuid(), "LOT-1", "Main Lot", "Australia/Sydney", new Money(12.5m, "AUD"));
        var bay = new ParkingBay(Guid.NewGuid(), lot.Id, new BayNumber("A1"), BayType.Standard);
        var reservation = Reservation.CreateHold(
            lot.Id,
            bay.Id,
            bay.BayNumber,
            bay.BayType,
            new TimeRange(now.AddHours(1), now.AddHours(3)),
            new LicensePlate("ABC123"),
            "Guest Tourist",
            "guest@example.com",
            true,
            new Money(25m, "AUD"),
            now,
            TimeSpan.FromMinutes(10));

        var reservationRepository = new Mock<IReservationRepository>();
        var bayRepository = new Mock<IParkingBayRepository>();
        var lotRepository = new Mock<IParkingLotRepository>();
        var paymentRepository = new Mock<IPaymentRepository>();
        var clock = new Mock<IDateTimeProvider>();

        reservationRepository.Setup(repository => repository.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);
        bayRepository.Setup(repository => repository.GetByIdAsync(bay.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bay);
        bayRepository.Setup(repository => repository.FindFirstAvailableCompatibleBayAsync(lot.Id, BayType.Standard, reservation.TimeRange, bay.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ParkingBay?)null);
        lotRepository.Setup(repository => repository.GetByIdAsync(lot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lot);
        paymentRepository.Setup(repository => repository.GetByReservationIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);
        clock.SetupGet(provider => provider.UtcNow).Returns(now);

        var handler = new ReassignBayCommandHandler(
            reservationRepository.Object,
            bayRepository.Object,
            lotRepository.Object,
            paymentRepository.Object,
            clock.Object);

        var result = await handler.Handle(new ReassignBayCommand(reservation.Id, null, "ControllerOverride", "Need manual decision"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.NeedsResolution.Should().BeTrue();
        result.Value.ResolutionNote.Should().Contain("Need manual decision");
    }
}
