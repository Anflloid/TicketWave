namespace TicketWave.Domain;

/// <summary>
/// Entity: бронювання місць. Усі правила бронювання живуть тут, а не в контролері.
/// </summary>
public sealed class Reservation
{
    /// <summary>Ліміт квитків на одну подію для одного акаунта (NFR-11).</summary>
    public const int MaxTicketsPerEvent = 4;

    /// <summary>Скільки часу місця тримаються за покупцем (NFR-06).</summary>
    public static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

    /// <summary>Страховка квитка - 10% від вартості місць.</summary>
    public const decimal InsuranceRate = 0.10m;

    private readonly List<EventSeat> _seats;

    private Reservation(Guid id, Guid userId, Guid eventId, List<EventSeat> seats, bool insurance,
        Money total, DateTimeOffset createdAt, DateTimeOffset reservedUntil)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        _seats = seats;
        Insurance = insurance;
        Total = total;
        CreatedAt = createdAt;
        ReservedUntil = reservedUntil;
        Status = ReservationStatus.Active;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid EventId { get; }
    public ReservationStatus Status { get; private set; }
    public IReadOnlyList<EventSeat> Seats => _seats;
    public bool Insurance { get; }
    public Money Total { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ReservedUntil { get; }

    /// <summary>
    /// Створити бронювання. Конструктор private, тому обійти ці перевірки неможливо.
    /// </summary>
    /// <param name="ticketsAlreadyOwned">Скільки квитків на цю подію в користувача вже є.</param>
    public static Reservation Create(Guid userId, Guid eventId, IReadOnlyCollection<EventSeat> seats,
        int ticketsAlreadyOwned, bool insurance, DateTimeOffset now)
    {
        if (seats.Count == 0 || seats.Count > MaxTicketsPerEvent)
            throw new DomainException($"Бронювання має містити від 1 до {MaxTicketsPerEvent} місць.");

        if (seats.Any(s => s.EventId != eventId))
            throw new DomainException("Усі місця мають належати одній події.");

        if (ticketsAlreadyOwned + seats.Count > MaxTicketsPerEvent)
            throw new DomainException(
                $"На одну подію можна мати не більше {MaxTicketsPerEvent} квитків, у вас уже {ticketsAlreadyOwned}.");

        var taken = seats.Where(s => s.Status != SeatStatus.Free).Select(s => s.Id).ToList();
        if (taken.Count > 0)
            throw new SeatAlreadyTakenException(taken);

        var reservedUntil = now + HoldDuration;
        foreach (var seat in seats)
            seat.Reserve(userId, reservedUntil);

        var seatsTotal = seats.Aggregate(Money.Zero, (sum, seat) => sum + seat.Price);
        var total = insurance ? seatsTotal + seatsTotal * InsuranceRate : seatsTotal;

        return new Reservation(Guid.NewGuid(), userId, eventId, seats.ToList(), insurance, total, now, reservedUntil);
    }

    public void Cancel()
    {
        if (Status != ReservationStatus.Active)
            throw new DomainException("Скасувати можна тільки активне бронювання.");

        foreach (var seat in _seats)
            seat.Release();

        Status = ReservationStatus.Cancelled;
    }
}
