using TicketWave.Domain;

namespace TicketWave.Application;

/// <summary>
/// Порт: повідомити схему залу, що статус місць змінився (ISeatNotifier з Component-діаграми ЛР2).
/// Справжня реалізація - SignalR, у цій роботі - запис у консоль.
/// </summary>
public interface ISeatNotifier
{
    Task SeatsChangedAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds, SeatStatus newStatus,
        CancellationToken ct = default);
}

/// <summary>
/// Запитаного ресурсу не існує (в API це 404). Не доменне правило, тому живе в Application.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
