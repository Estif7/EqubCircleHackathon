# EkubCircle Backend - Simple Clean Architecture

## Structure

```text
API            -> Controllers, middleware, HTTP configuration
Application    -> DTOs, interfaces, use-case/business services
Domain         -> Entities and enums; no infrastructure dependencies
Infrastructure -> EF Core/PostgreSQL, JWT, hashing, persistence
```

Dependency direction:

```text
API -> Application -> Domain
 |        |
 └------> Infrastructure -> Domain
          
Application does not depend on Infrastructure.
```

## Run

```bash
docker compose up -d postgres
dotnet restore
dotnet ef database update --project Infrastructure --startup-project API
dotnet run --project API
```

Swagger is available at the HTTPS URL printed by ASP.NET Core.

## Demo accounts

- Organizer: organizer@hackathon.local / Organizer123!
- Member: member@hackathon.local / Member123!

## Hackathon focus

The backend prioritizes the core Ekub rules: verified users, circle membership, fixed payout order, one payment per member per round, all-members-paid requirement, one payout per round, no member receiving twice, and circle completion.
