using TicketWave.Domain;

namespace TicketWave.Infrastructure;

/// <summary>Адаптер порту IEventSeatRepository поверх сховища в пам'яті.</summary>
public sealed class InMemoryEventSeatRepository(InMemoryTicketStore store) : IEventSeatRepository
{
    public Task<IReadOnlyList<EventSeat>> GetByIdsAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds,
        CancellationToken ct = default)
    {
        lock (store.SyncRoot)
        {
            IReadOnlyList<EventSeat> result = seatIds
                .Where(id => store.Seats.TryGetValue(id, out var r) && r.EventId == eventId)
                .Select(id => ToDomain(store.Seats[id]))
                .ToList();
            return Task.FromResult(result);
        }
    }

    private static EventSeat ToDomain(SeatRecord r) =>
        EventSeat.Restore(r.Id, r.EventId, r.SectorName, r.Row, r.Number, new Money(r.Price),
            r.Status, r.ReservedBy, r.ReservedUntil);
}

/// <summary>Адаптер порту IReservationRepository поверх сховища в пам'яті.</summary>
public sealed class InMemoryReservationRepository(InMemoryTicketStore store) : IReservationRepository
{
    public Task SaveAsync(Reservation reservation, CancellationToken ct = default)
    {
        lock (store.SyncRoot)
        {
            // аналог UPDATE seats ... WHERE status = 'FREE' з ADR-0002:
            // якщо між читанням і записом місце вже хтось забрав - нічого не записується
            var isNew = !store.Reservations.ContainsKey(reservation.Id);
            if (isNew)
            {
                var taken = reservation.Seats
                    .Where(s => store.Seats[s.Id].Status != SeatStatus.Free)
                    .Select(s => s.Id)
                    .ToList();
                if (taken.Count > 0)
                    throw new SeatAlreadyTakenException(taken);
            }

            foreach (var seat in reservation.Seats)
            {
                store.Seats[seat.Id] = store.Seats[seat.Id] with
                {
                    Status = seat.Status,
                    ReservedBy = seat.ReservedBy,
                    ReservedUntil = seat.ReservedUntil
                };
            }

            store.Reservations[reservation.Id] = reservation;
        }

        return Task.CompletedTask;
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (store.SyncRoot)
        {
            return Task.FromResult(store.Reservations.GetValueOrDefault(id));
        }
    }

    public Task<int> CountTicketsAsync(Guid userId, Guid eventId, CancellationToken ct = default)
    {
        lock (store.SyncRoot)
        {
            var count = store.Reservations.Values
                .Where(r => r.UserId == userId && r.EventId == eventId)
                .Where(r => r.Status is ReservationStatus.Active or ReservationStatus.Paid)
                .Sum(r => r.Seats.Count);
            return Task.FromResult(count);
        }
    }
}
