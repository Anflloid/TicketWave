using TicketWave.Domain;

namespace TicketWave.Application.Reservations;

/// <summary>Вхідні дані сценарію (POST /reservations з контракту ЛР3).</summary>
public sealed record CreateReservationCommand(Guid UserId, Guid EventId, IReadOnlyList<Guid> SeatIds, bool Insurance);

/// <summary>Результат сценарію.</summary>
public sealed record CreateReservationResult(Guid ReservationId, decimal Total, string Currency,
    DateTimeOffset ReservedUntil, int SeatsCount);

/// <summary>
/// Сценарій «Забронювати місця». Усі залежності - інтерфейси в конструкторі,
/// жодного new адаптера і жодного GetService усередині.
/// </summary>
public sealed class CreateReservationUseCase(
    IEventSeatRepository seats,          // порт: читання місць
    IReservationRepository reservations, // порт: збереження бронювань
    ISeatNotifier notifier,              // порт: оновлення схеми залу
    TimeProvider clock)                  // поточний час (у тестах - фіксований)
{
    public async Task<CreateReservationResult> ExecuteAsync(CreateReservationCommand command,
        CancellationToken ct = default)
    {
        var seatIds = command.SeatIds.Distinct().ToList();

        var foundSeats = await seats.GetByIdsAsync(command.EventId, seatIds, ct);
        if (foundSeats.Count != seatIds.Count)
            throw new NotFoundException("Одне або кілька місць не знайдено на цій події.");

        var alreadyOwned = await reservations.CountTicketsAsync(command.UserId, command.EventId, ct);

        // усі правила (ліміт 4 квитки, місце вільне, від 1 до 4 місць) перевіряє домен
        var reservation = Reservation.Create(command.UserId, command.EventId, foundSeats, alreadyOwned,
            command.Insurance, clock.GetUtcNow());

        await reservations.SaveAsync(reservation, ct);
        await notifier.SeatsChangedAsync(command.EventId, seatIds, SeatStatus.Reserved, ct);

        return new CreateReservationResult(reservation.Id, reservation.Total.Amount, reservation.Total.Currency,
            reservation.ReservedUntil, reservation.Seats.Count);
    }
}
