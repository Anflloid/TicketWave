using Microsoft.AspNetCore.Mvc;
using TicketWave.Application;
using TicketWave.Application.Reservations;
using TicketWave.Domain;
using TicketWave.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ===================== Композиційний корінь =====================
// Усі реєстрації тільки тут. Domain і Application цей файл не бачать.

// Singleton: системний годинник не має стану, один на весь процес.
// Scoped/Transient нічого б не дали - об'єкт однаковий для всіх запитів.
builder.Services.AddSingleton(TimeProvider.System);

// Singleton: це і є «база» в пам'яті. Зі Scoped кожен запит бачив би порожнє сховище,
// а з Transient - навіть кожен репозиторій окремо.
builder.Services.AddSingleton<InMemoryTicketStore>();

// Scoped: один репозиторій на HTTP-запит. Зі справжньою БД (PostgreSQL з ADR-0002)
// обидва репозиторії ділили б одне з'єднання і одну транзакцію в межах запиту.
// Singleton не можна - тримав би з'єднання одного запиту для всіх.
builder.Services.AddScoped<IEventSeatRepository, InMemoryEventSeatRepository>();
builder.Services.AddScoped<IReservationRepository, InMemoryReservationRepository>();

// Transient: не має стану між викликами і дешевий у створенні, тому новий на кожне вирішення.
builder.Services.AddTransient<ISeatNotifier, ConsoleSeatNotifier>();

// Scoped: сценарій живе в межах запиту разом зі своїми Scoped-репозиторіями.
// Singleton заборонений: це captive dependency, .NET у Development впаде на старті.
builder.Services.AddScoped<CreateReservationUseCase>();

var app = builder.Build();

// ===================== Ендпоінти =====================
// Контролер тільки перекладає запит у команду, викликає сценарій і перекладає результат у відповідь.

app.MapPost("/reservations", async (CreateReservationRequest request, HttpContext http,
    CreateReservationUseCase useCase, CancellationToken ct) =>
{
    // Keycloak (ADR-0003) підключу пізніше, поки користувач передається заголовком
    if (!Guid.TryParse(http.Request.Headers["X-User-Id"].ToString(), out var userId))
        return Problem(http, 401, "Unauthorized", "Не передано або неправильний заголовок X-User-Id.", "unauthorized");

    var command = new CreateReservationCommand(userId, request.EventId, request.SeatIds ?? [], request.Insurance);

    try
    {
        var result = await useCase.ExecuteAsync(command, ct);
        return Results.Created($"/reservations/{result.ReservationId}", result);
    }
    catch (SeatAlreadyTakenException ex)
    {
        return Problem(http, 409, "Seat already taken", ex.Message, "seat-taken",
            p => p.Extensions["conflictingSeatIds"] = ex.SeatIds);
    }
    catch (DomainException ex)
    {
        return Problem(http, 422, "Business rule violated", ex.Message, "business-rule");
    }
    catch (NotFoundException ex)
    {
        return Problem(http, 404, "Not found", ex.Message, "not-found");
    }
});

app.Run();

// Відповідь помилки у форматі RFC 7807, як у контракті ЛР3
static IResult Problem(HttpContext http, int status, string title, string detail, string problemType,
    Action<ProblemDetails>? extra = null)
{
    var problem = new ProblemDetails
    {
        Type = $"https://api.ticketwave.example/problems/{problemType}",
        Title = title,
        Status = status,
        Detail = detail,
        Instance = http.Request.Path
    };
    extra?.Invoke(problem);
    return Results.Problem(problem);
}

/// <summary>Тіло запиту POST /reservations (CreateReservationRequest з openapi.yaml).</summary>
public sealed record CreateReservationRequest(Guid EventId, List<Guid>? SeatIds, bool Insurance);
