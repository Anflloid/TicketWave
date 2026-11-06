namespace TicketWave.Domain;

/// <summary>
/// Порт: звідки брати місця на події. Реалізація (адаптер) живе в Infrastructure.
/// </summary>
public interface IEventSeatRepository
{
    Task<IReadOnlyList<EventSeat>> GetByIdsAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds,
        CancellationToken ct = default);
}

/// <summary>
/// Порт: збереження бронювань разом зі статусами їхніх місць.
/// </summary>
public interface IReservationRepository
{
    Task SaveAsync(Reservation reservation, CancellationToken ct = default);

    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Скільки квитків (активні й оплачені бронювання) користувач уже має на подію.</summary>
    Task<int> CountTicketsAsync(Guid userId, Guid eventId, CancellationToken ct = default);
}
