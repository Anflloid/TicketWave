namespace TicketWave.Domain;

/// <summary>
/// Value object: сума грошей. Не має Id, порівнюється за значенням, незмінний.
/// </summary>
public sealed record Money
{
    public static Money Zero { get; } = new(0m);

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "UAH")
    {
        if (amount < 0)
            throw new DomainException("Сума не може бути від'ємною.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Валюта обов'язкова.");

        Amount = decimal.Round(amount, 2);
        Currency = currency;
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Не можна додавати суми в різних валютах.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal factor) => new(a.Amount * factor, a.Currency);
}
