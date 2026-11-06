using Moq;
using TicketWave.Application;
using TicketWave.Application.Reservations;
using TicketWave.Domain;
using Xunit;

namespace TicketWave.Application.Tests;

public class CreateReservationUseCaseTests
{
    private static readonly Guid EventId = Guid.Parse("9c4e1a7b-2f3d-4e6a-8b1c-7d2e5f9a3c01");
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 10, 0, 0, TimeSpan.Zero);

    // дублери портів замість справжнього сховища і SignalR
    private readonly Mock<IEventSeatRepository> _seats = new();
    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<ISeatNotifier> _notifier = new();

    // SUT - справжній сценарій, у конструктор якого передані дублери
    private CreateReservationUseCase CreateSut() =>
        new(_seats.Object, _reservations.Object, _notifier.Object, new FixedTimeProvider(Now));

    [Fact]
    public async Task Execute_FreeSeats_ReservesAndSavesThroughRepository()
    {
        var seat12 = FreeSeat(12);
        var seat13 = FreeSeat(13);
        GivenSeats(seat12, seat13);
        GivenUserAlreadyOwns(0);

        var result = await CreateSut().ExecuteAsync(Command(seat12, seat13));

        Assert.Equal(2400m, result.Total);              // 2 x 1200
        Assert.Equal(Now.AddMinutes(10), result.ReservedUntil);
        Assert.Equal(SeatStatus.Reserved, seat12.Status);
        _reservations.Verify(r => r.SaveAsync(
            It.Is<Reservation>(x => x.Id == result.ReservationId && x.Seats.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.SeatsChangedAsync(EventId, It.IsAny<IReadOnlyCollection<Guid>>(),
            SeatStatus.Reserved, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_TicketLimitExceeded_IsRejectedByDomain_AndNothingIsSaved()
    {
        var seat12 = FreeSeat(12);
        var seat13 = FreeSeat(13);
        GivenSeats(seat12, seat13);
        GivenUserAlreadyOwns(3);                         // 3 + 2 > 4

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => CreateSut().ExecuteAsync(Command(seat12, seat13)));

        Assert.Contains("не більше 4", ex.Message);
        _reservations.Verify(r => r.SaveAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notifier.Verify(n => n.SeatsChangedAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<SeatStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_RepositoryFails_ErrorReachesUseCase_AndNoNotificationIsSent()
    {
        var seat12 = FreeSeat(12);
        GivenSeats(seat12);
        GivenUserAlreadyOwns(0);
        _reservations
            .Setup(r => r.SaveAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSut().ExecuteAsync(Command(seat12)));

        Assert.Equal("Storage unavailable", ex.Message);
        _notifier.Verify(n => n.SeatsChangedAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<SeatStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_SeatAlreadyReserved_IsRejectedByDomain_AndNothingIsSaved()
    {
        var takenSeat = FreeSeat(12);
        takenSeat.Reserve(Guid.NewGuid(), Now.AddMinutes(5));   // місце вже забронював інший покупець
        GivenSeats(takenSeat);
        GivenUserAlreadyOwns(0);

        var ex = await Assert.ThrowsAsync<SeatAlreadyTakenException>(
            () => CreateSut().ExecuteAsync(Command(takenSeat)));

        Assert.Contains(takenSeat.Id, ex.SeatIds);
        _reservations.Verify(r => r.SaveAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ---------- допоміжні методи ----------

    private static EventSeat FreeSeat(int number) =>
        new(Guid.NewGuid(), EventId, "Фан-зона A", 5, number, new Money(1200m));

    private static CreateReservationCommand Command(params EventSeat[] seats) =>
        new(UserId, EventId, seats.Select(s => s.Id).ToList(), Insurance: false);

    private void GivenSeats(params EventSeat[] seats) =>
        _seats.Setup(r => r.GetByIdsAsync(EventId, It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(seats);

    private void GivenUserAlreadyOwns(int tickets) =>
        _reservations.Setup(r => r.CountTicketsAsync(UserId, EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

    /// <summary>Годинник, який завжди показує один і той самий час.</summary>
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
