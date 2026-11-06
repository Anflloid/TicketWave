namespace TicketWave.Domain;

/// <summary>
/// Entity: місце на конкретній події (EventSeat з class-діаграми ЛР2).
/// Ідентичність - Id. Статус змінюється тільки через методи, а не «руками» ззовні.
/// </summary>
public sealed class EventSeat
{
    public EventSeat(Guid id, Guid eventId, string sectorName, int row, int number, Money price)
    {
        if (string.IsNullOrWhiteSpace(sectorName))
            throw new DomainException("Назва сектора обов'язкова.");
        if (row < 1 || number < 1)
            throw new DomainException("Ряд і номер місця мають бути більше нуля.");

        Id = id;
        EventId = eventId;
        SectorName = sectorName;
        Row = row;
        Number = number;
        Price = price;
        Status = SeatStatus.Free;
    }

    public Guid Id { get; }
    public Guid EventId { get; }
    public string SectorName { get; }
    public int Row { get; }
    public int Number { get; }
    public Money Price { get; }
    public SeatStatus Status { get; private set; }
    public Guid? ReservedBy { get; private set; }
    public DateTimeOffset? ReservedUntil { get; private set; }

    /// <summary>
    /// Відновити місце зі сховища з уже відомим статусом (використовує репозиторій).
    /// </summary>
    public static EventSeat Restore(Guid id, Guid eventId, string sectorName, int row, int number, Money price,
        SeatStatus status, Guid? reservedBy, DateTimeOffset? reservedUntil)
    {
        var seat = new EventSeat(id, eventId, sectorName, row, number, price);
        seat.Status = status;
        seat.ReservedBy = reservedBy;
        seat.ReservedUntil = reservedUntil;
        return seat;
    }

    public void Reserve(Guid userId, DateTimeOffset until)
    {
        if (Status != SeatStatus.Free)
            throw new SeatAlreadyTakenException([Id]);

        Status = SeatStatus.Reserved;
        ReservedBy = userId;
        ReservedUntil = until;
    }

    public void Release()
    {
        if (Status != SeatStatus.Reserved)
            throw new DomainException("Звільнити можна тільки заброньоване місце.");

        Status = SeatStatus.Free;
        ReservedBy = null;
        ReservedUntil = null;
    }

    public void MarkSold()
    {
        if (Status != SeatStatus.Reserved)
            throw new DomainException("Продати можна тільки заброньоване місце.");

        Status = SeatStatus.Sold;
        ReservedUntil = null;
    }
}
