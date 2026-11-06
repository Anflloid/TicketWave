# TicketWave - сервіс бронювання квитків на події

Навчальний проєкт з дисципліни «Проєктування архітектури, специфікація та стандарти інженерії».

**Варіант 4:** Online Ticket Booking & Event Platform (Сервіс бронювання квитків).

## Коротко про проєкт

TicketWave - це вебплатформа, де можна купити квитки на концерти та спортивні події. Користувач обирає подію, бачить інтерактивну схему залу, вибирає одне конкретне місце і оплачує його онлайн. Окремо в системі є захист від перекупників (обмеження кількості квитків, перевірка на ботів та черга при старті продажів).

## Виконавець

| ПІБ | Група | Роль у проєкті |
|-----|-------|----------------|
| Киселевич Андрій Юрійович |3ПР| Виконує проєкт самостійно (архітектура, документація, розробка) |

## Лабораторні роботи

| Тег | Лабораторна | Головні артефакти |
|-----|-------------|-------------------|
| `Lr1` | Вимоги, NFR, QAS, ADR | `docs/1_system_description.md` ... `docs/4_viewpoints_map.md`, `docs/adrs/` |
| `lr2` | C4 Model, UML | `diagrams/*.puml`, `docs/images/`, `docs/architecture_diagrams_report.md` |
| `lr3` | OpenAPI 3.0, gRPC Proto3 | `api/openapi.yaml`, `api/service.proto`, `src/TicketWave.Contracts/`, `docs/api_specification_report.md` |
| `lr4` | Clean Architecture, DI | `src/Domain`, `src/Application`, `src/Infrastructure`, `src/WebAPI`, `tests/`, `docs/clean_architecture_report.md` |

## Структура репозиторію

```
TicketWave/
├── README.md
├── .spectral.yaml                     # Правила лінтера OpenAPI (ЛР3)
├── api/                               # Контракти API (ЛР3)
│   ├── openapi.yaml                   # Публічний REST API, OpenAPI 3.0.3
│   └── service.proto                  # Внутрішній gRPC API, proto3
├── TicketWave.sln                     # Рішення з усіма .NET-проєктами
├── src/
│   ├── Domain/                        # Сутності, value objects, порти репозиторіїв (ЛР4)
│   ├── Application/                   # Сценарій CreateReservationUseCase, DTO (ЛР4)
│   ├── Infrastructure/                # Адаптери: сховище в пам'яті, notifier (ЛР4)
│   ├── WebAPI/                        # Program.cs: композиційний корінь, ендпоінт (ЛР4)
│   └── TicketWave.Contracts/          # .NET-проєкт, генерує gRPC-стаби з api/service.proto
│       └── Generated/                 # Згенеровані Service.cs і ServiceGrpc.cs
├── tests/
│   └── TicketWave.Application.Tests/  # Модульні тести сценарію, xUnit + Moq (ЛР4)
├── benchmark/
│   └── payload_size.py                # Порівняння JSON vs Protobuf (ЛР3)
├── diagrams/                          # Вихідні файли PlantUML (ЛР2)
│   ├── c4_context.puml
│   ├── c4_container.puml
│   ├── component_booking.puml
│   ├── class_domain.puml
│   ├── sequence_booking_payment.puml
│   ├── sequence_queue_seatmap.puml
│   └── state_event_seat.puml
└── docs/
    ├── 1_system_description.md        # Опис системи, межі та матриця стейкхолдерів
    ├── 2_nfr_specification.md         # Специфікація NFR за ISO 25010:2023
    ├── 3_quality_scenarios.md         # Сценарії якісних атрибутів (QAS)
    ├── 4_viewpoints_map.md            # Карта ракурсів за ISO 42010:2022
    ├── architecture_diagrams_report.md # Звіт ЛР2 з усіма діаграмами
    ├── api_specification_report.md    # Звіт ЛР3
    ├── clean_architecture_report.md   # Звіт ЛР4
    ├── dependency_check.log           # Вивід dotnet list reference (ЛР4)
    ├── test_run.log                   # Лог прогону тестів (ЛР4)
    ├── images/                     # PNG діаграм (ЛР2) і скріншоти (images/lr3/)
    │   ├── lr3/     # Скріншоти ЛР3
    │   │   ├── dotnet_build_stubs.png
    │   │   ├── postman_get_seats.png
    │   │   ├── postman_post_201.png
    │   │   ├── postman_post_422.png
    │   │   └── prism_start.png
    │   ├── lr4/                       # Скріншоти ЛР4
    │   │   └── test_run.png
    │   ├── c4_context.png             # Діаграми ЛР2
    │   ├── c4_container.png
    │   ├── component_booking.png
    │   ├── class_domain.png
    │   ├── sequence_booking_payment.png
    │   ├── sequence_queue_seatmap.png
    │   └── state_event_seat.png
    └── adrs/
        ├── 0001-architectural-style.md
        ├── 0002-database-strategy.md
        └── 0003-security-framework.md
 
```
