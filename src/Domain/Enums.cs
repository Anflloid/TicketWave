namespace TicketWave.Domain;

public enum SeatStatus
{
    Free,
    Reserved,
    Sold,
    Blocked
}

public enum ReservationStatus
{
    Active,
    Paid,
    Cancelled,
    Expired
}
