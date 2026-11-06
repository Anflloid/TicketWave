namespace TicketWave.Domain;

/// <summary>
/// Виняток, яким домен каже «ні»: операція порушує правило предметної області.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

/// <summary>
/// Одне або кілька місць уже зайняті. Окремий тип, бо в API це 409, а не 422.
/// </summary>
public sealed class SeatAlreadyTakenException : DomainException
{
    public SeatAlreadyTakenException(IReadOnlyList<Guid> seatIds)
        : base("Одне або кілька місць уже зайняті.")
    {
        SeatIds = seatIds;
    }

    public IReadOnlyList<Guid> SeatIds { get; }
}
