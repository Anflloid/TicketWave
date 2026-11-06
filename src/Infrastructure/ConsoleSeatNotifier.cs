using TicketWave.Application;
using TicketWave.Domain;

namespace TicketWave.Infrastructure;

/// <summary>
/// Адаптер порту ISeatNotifier. Пізніше тут буде SignalR, зараз - просто вивід у консоль.
/// </summary>
public sealed class ConsoleSeatNotifier : ISeatNotifier
{
    public Task SeatsChangedAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds, SeatStatus newStatus,
        CancellationToken ct = default)
    {
        Console.WriteLine($"[SeatNotifier] подія {eventId}: {seatIds.Count} місць -> {newStatus}");
        return Task.CompletedTask;
    }
}
