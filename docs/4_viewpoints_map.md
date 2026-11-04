# 4. Карта архітектурних ракурсів (Architectural Viewpoints Map) за ISO/IEC/IEEE 42010:2022

За стандартом ISO/IEC/IEEE 42010:2022 **ракурс (Viewpoint)** - це правила і нотація, за якими будується опис, а **вигляд (View)** - це конкретна діаграма системи, яка була побудована за цими правилами. Кожен вигляд потрібен, щоб відповісти на занепокоєння (Concerns) певних стейкхолдерів.

## 4.1. Таблиця відповідності

| Viewpoint (Ракурс) | View (Вигляд / Артефакт) | Нотація | Зацікавлені сторони (Stakeholders) | Які Concerns закриває |
|---|---|---|---|---|
| Context Viewpoint | C4 Level 1: System Context Diagram - TicketWave і зовнішні системи (платіжний шлюз, email, push, сканери на вході) | C4 Model / Structurizr | Business Owner, End User, Event Organizer, Payment Provider | Межі системи, з якими зовнішніми системами інтегрується |
| Container Viewpoint | C4 Level 2: Container Diagram - Web SPA на Angular, мобільний застосунок, API Gateway, Backend на ASP.NET Core (модульний моноліт), PostgreSQL, Redis, RabbitMQ, Keycloak | C4 Model / PlantUML | Software Architect, Lead Developer, DevOps | Які технології використовую, як контейнери спілкуються між собою |
| Structural Viewpoint | UML Component Diagram модулів (Catalog, Venue, Booking, Queue, Payment, Ticket, Notification) та Class Diagram доменної моделі бронювання | UML 2.5 | Developers, Module Owners | Модульність, залежності між модулями, доменна модель |
| Behavioral Viewpoint | UML Sequence Diagram «Бронювання і оплата місця»; State Diagram статусів місця (FREE → RESERVED → SOLD / FREE) | UML 2.5 | Backend Developers, QA Engineers | Як система обробляє бронювання, як уникнути подвійного продажу, що відбувається після таймауту |
| Deployment Viewpoint | UML Deployment Diagram - Kubernetes кластер у хмарі, поди, HPA, керовані БД, CDN | UML 2.5 / C4 Deployment | DevOps, Infrastructure Engineers, Security Officer | Масштабування, відмовостійкість, розміщення в хмарі |
| Security Viewpoint | Діаграма потоків автентифікації (OAuth2 / OIDC через Keycloak) та зон довіри (DMZ, внутрішня мережа) | UML Sequence / Data Flow Diagram | Security Officer, DevOps, Developers | Захист даних, автентифікація, захист від ботів |

## 4.2. Зв'язок ракурсів із документами проєкту

| Ракурс | Пов'язані NFR / QAS / ADR |
|---|---|
| Context Viewpoint | NFR-08, QAS-3 (інтеграція з платіжним шлюзом) |
| Container Viewpoint | ADR-0001, ADR-0002, ADR-0003 |
| Structural Viewpoint | NFR-14, NFR-15, QAS-5, ADR-0001 |
| Behavioral Viewpoint | NFR-05, NFR-06, QAS-2 |
| Deployment Viewpoint | NFR-03, NFR-07, NFR-16, QAS-1, QAS-6 |
| Security Viewpoint | NFR-10 - NFR-13, QAS-4, ADR-0003 |

> Самі діаграми будуть зроблені в наступних лабораторних роботах. У цій роботі визначено, які саме вигляди потрібні і для кого.
