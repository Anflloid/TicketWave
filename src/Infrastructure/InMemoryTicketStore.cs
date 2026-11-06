using TicketWave.Domain;

namespace TicketWave.Infrastructure;

/// <summary>
/// «База даних» у пам'яті процесу. Справжня БД у цій роботі не піднімається.
/// Місця зберігаються як прості записи, щоб домен отримував копії, а не спільні об'єкти.
/// </summary>
public sealed class InMemoryTicketStore
{
    public static readonly Guid DemoEventId = Guid.Parse("9c4e1a7b-2f3d-4e6a-8b1c-7d2e5f9a3c01");

    public object SyncRoot { get; } = new();

    public Dictionary<Guid, SeatRecord> Seats { get; } = new();

    public Dictionary<Guid, Reservation> Reservations { get; } = new();

    public InMemoryTicketStore()
    {
        // демо-подія з тими самими id місць, що й у прикладах openapi.yaml з ЛР3
        AddSeat("3f8a2c1d-6e4b-4a9f-b2d7-1c5e8f0a9b22", 12);
        AddSeat("7a1b9e3c-2d4f-4b8a-9c6e-0f1d2e3a4b55", 13);
        AddSeat("c2d9f4a1-8b3e-4f6c-9a7d-5e1b2c3d4e66", 14);
        AddSeat("e5a7b3c9-1d2f-4e8a-b6c4-9f0a1b2c3d77", 15);
        AddSeat("11111111-1111-4111-8111-111111111111", 16);
    }

    private void AddSeat(string id, int number)
    {
        var seatId = Guid.Parse(id);
        Seats[seatId] = new SeatRecord(seatId, DemoEventId, "Фан-зона A", 5, number, 1200m,
            SeatStatus.Free, null, null);
    }
}

/// <summary>Як місце лежить у сховищі (аналог рядка таблиці seats).</summary>
public sealed record SeatRecord(Guid Id, Guid EventId, string SectorName, int Row, int Number, decimal Price,
    SeatStatus Status, Guid? ReservedBy, DateTimeOffset? ReservedUntil);
