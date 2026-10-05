# EkubCircle — Rotating Savings and Credit Association (ROSCA)

## Team Members & Responsibilities
- Estifanos Dejene (@Estifc137) — Project Lead, Architecture, Docker & Solution Setup
- Samuel Setarige (@samvode27) — Domain Models, Database Architecture & EF Core Infrastructure
- Nahom (@Nahom2231) — Application Services, Business Logic & DTO Contracts
- Mike Eten (@mikeeten) — ASP.NET Core Web API, SignalR Hub & E2E Circle Integrations
- Selamsew Alemu (@selamsewalemu) — Angular Web Portal, Real-time Dashboard & UI Design
- Wongel Lakew (@wongellakew-lab) — Flutter Cross-Platform Mobile Client

## Tech Stack Used
- Frontend: Angular 20
- Backend: ASP.NET Core Web API .NET 8
- Database: EF Core with PostgreSQL

## How to Run Locally

### Backend Setup
1. cd API
2. dotnet restore
3. dotnet ef database update
4. dotnet run

> **Note**: Start PostgreSQL before launching the API (`docker compose up -d postgres`). For a fresh database, you can also apply `schema.sql` directly to the `ekubcircle` database.

### Frontend Setup
1. cd EkubCircleFrontend
2. npm install
3. ng serve

> **Note**: Angular dev server runs on `http://localhost:4200` and proxies `/api/*` and `/hubs/*` to `http://localhost:5000` via `proxy.conf.json`.

### Mobile Setup (Optional)
1. cd ekub_circle_mobile
2. flutter pub get
3. flutter run

## Test Accounts & Demo Credentials
- Admin / Staff User: organizer@hackathon.local / Organizer123!
- Standard User: member@hackathon.local / Member123!

> Both accounts are pre-seeded in the database with verified status. You can also register a new user through the registration flow with simulated Ethiopian National ID (Fayda) OTP verification.

## Working Features
- **Authentication & Simulated Fayda Identity Verification**:
  - Registration with Ethiopian phone number, email, and Fayda National ID (FAN).
  - Simulated Fayda OTP verification flow with SHA-256 FAN hashing and 6-digit OTP verification.
  - JWT Bearer token authentication and role-based session management.
- **Circle (Ekub) Creation & Lifecycle**:
  - Circle configuration: Name, contribution amount (ETB), cycle frequency (Weekly/Monthly), member limit, and optional start date.
  - Complete lifecycle state transitions: `Open` -> `Active` -> `Completed`.
  - Available circles listing with search and frequency filtering.
  - One-click seat joining and organizer member invitations by phone or email.
- **Circle Start Lock & Fixed Payout Sequence**:
  - Capacity enforcement: Circle can only start once the active member count reaches the configured member limit.
  - Starting the circle locks the payout order deterministically and generates rounds corresponding to each member.
- **Contribution Tracking & Member Payment Flow**:
  - Real-time round tracking (current pot size in ETB, assigned round receiver, and member payment status).
  - Self-service member payment: Members can pay their own contribution directly from the dashboard.
  - Organizer payment recording: Organizers can record and verify contributions on behalf of any circle member.
  - Validation guards ensuring exact contribution amounts and preventing duplicate payments for the same round.
- **Round Payout Distribution**:
  - Pot distribution validation: Organizers can trigger payout to the designated round receiver once all circle members have contributed.
  - Strict rule enforcement preventing duplicate payouts in the same round or members receiving payouts twice.
  - Automatic progression to the next round until all members have received their payout.
- **Real-Time Updates via SignalR**:
  - SignalR Hub (`/hubs/notifications`) broadcasting events for new circles, member joined, circle started, payment recorded, and payout completed.
  - Real-time dashboard and circle detail page updates without requiring manual browser refresh.
  - Toast notifications for live circle events.
- **Member Dashboard**:
  - Summary metrics: Active Circles, Total Contributions, and Accumulated Payouts.
  - Actionable "Next Payment Circle" and "Next Payout Circle" cards indicating pending dues and expected payouts.
- **Cross-Platform Mobile App (Flutter)**:
  - Feature-complete mobile client supporting auth, OTP verification, dashboard, circle exploration, contribution submission, and round audit history.

## Known Limitations & Bugs
- **Simulated Third-Party Integrations**:
  - Fayda National ID verification and SMS OTP delivery are simulated in-memory rather than connected to live production APIs (Ethiopian National ID Program / Ethio Telecom SMS gateway).
  - Payment processing and payout disbursement are managed via software ledger entries; live telebirr, CBE Birr, or Chapa payment gateways are not yet integrated.
- **Lottery / Random Draw Mechanism**:
  - Payout ordering is currently assigned deterministically upon circle start; an interactive live lottery / raffle draw animation for randomized payout ordering was not completed.
- **Guarantor & Collateral Workflow**:
  - Traditional community guarantor workflows (requiring peer guarantees before early round payout disbursement) were scoped out for the initial demo.
- **Automated Overdue Penalty System**:
  - Automated late fee calculations and reminder notifications for overdue members are not yet implemented.