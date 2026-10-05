# Circle creation fix

## Backend
CircleService now normalizes the optional StartDate to `DateTimeKind.Utc` before EF Core saves it. This fixes Npgsql's `timestamp with time zone` error caused by an Angular date input deserializing as `DateTimeKind.Unspecified`.

## Frontend
The create-circle form now:
- trims and validates the circle name before sending;
- sends contribution/member limit as numbers;
- sends the date as an explicit UTC ISO timestamp (`T00:00:00.000Z`).

Restart both applications after replacing the backend/frontend files.
