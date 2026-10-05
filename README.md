# EkubCircle

EkubCircle helps a group save together. Members contribute on a regular schedule and take turns receiving the shared pot.

The project includes a .NET API, an Angular web app, a Flutter mobile app, and a PostgreSQL database. Fayda and OTP verification are simulated for the demo.

## Run it locally

You'll need .NET 8, Node.js with npm, and Docker.

1. Start PostgreSQL: `docker compose up -d postgres`.
2. For a new database, apply `schema.sql` to the `ekubcircle` database.
3. Check the API connection string in `API/appsettings.Development.json`. The compose file uses the password `postgres`; update the connection string if your local password differs.
4. Start the API: `dotnet run --project API/EkubCircle.API.csproj`.
5. In another terminal, run `cd EkubCircleFrontend`, `npm install`, then `npm start`.
6. Open `http://localhost:4200`.

### Mobile (Flutter)

```bash
cd ekub_circle_mobile
flutter pub get
flutter run
```

## Team & Contributors

| Name / GitHub | Role / Focus Area |
| --- | --- |
| [@Estifc137](https://github.com/Estifc137) | Project Lead, Architecture, Docker & Solution Setup |
| [@samvode27](https://github.com/samvode27) | Domain Models, Database Architecture & EF Core Infrastructure |
| [@Nahom2231](https://github.com/Nahom2231) | Application Services, Business Logic & DTO Contracts |
| [@mikeeten](https://github.com/mikeeten) | ASP.NET Core Web API, SignalR Hub & E2E Circle Integrations |
| [@selamsewalemu](https://github.com/selamsewalemu) | Angular Web Portal, Real-time Dashboard & UI Design |
| [@wongellakew-lab](https://github.com/wongellakew-lab) | Flutter Cross-Platform Mobile Client |