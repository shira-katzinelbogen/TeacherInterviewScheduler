# TeacherInterviewScheduler

ASP.NET Core Web API where the teacher schedules interviews for students, and students can submit/manage their availability.

This service stores interview slots, student availability, and scheduled interviews in **SQL Server** using **Entity Framework Core** migrations.

## Tech stack

- **.NET**: `net8.0`
- **Web**: ASP.NET Core (minimal hosting)
- **DB**: SQL Server + EF Core migrations
- **Mapping**: AutoMapper

## Prerequisites

- **.NET 8 SDK** (or newer that can build `net8.0`)
- **SQL Server** (any of the following):
  - SQL Server Express / Developer
  - LocalDB
  - A remote SQL Server instance you can connect to

## Configuration

The API **requires** a connection string environment variable at startup:

- **`SCHEDULING_DB_CS`**: SQL Server connection string

If it is not set, the app throws:
`InvalidOperationException: The SCHEDULING_DB_CS environment variable is not set.`

### Example connection strings

- **SQL Server Express (Windows auth)**

`Data Source=.\SQLEXPRESS;Initial Catalog=SchedulingDB;Integrated Security=True;TrustServerCertificate=True`

- **LocalDB**

`Server=(localdb)\MSSQLLocalDB;Database=SchedulingDB;Trusted_Connection=True;TrustServerCertificate=True`

## Run locally (PowerShell)

From the repo root:

```powershell
# 1) Set the DB connection string for this shell session
$env:SCHEDULING_DB_CS = "Server=(localdb)\MSSQLLocalDB;Database=SchedulingDB;Trusted_Connection=True;TrustServerCertificate=True"

# 2) Restore & build
dotnet restore
dotnet build -c Release

# 3) Apply database migrations (creates/updates tables)
dotnet ef database update --project .\SchedulingService.csproj

# 4) Run the API
dotnet run --project .\SchedulingService.csproj
```

When it starts successfully, you should see it listening (by default) on:

- `http://localhost:5000`

## Run locally (CMD)

```bat
REM 1) Set the DB connection string for this shell session
set "SCHEDULING_DB_CS=Server=(localdb)\MSSQLLocalDB;Database=SchedulingDB;Trusted_Connection=True;TrustServerCertificate=True"

REM 2) Apply migrations + run
dotnet ef database update --project .\SchedulingService.csproj
dotnet run --project .\SchedulingService.csproj
```

## Database schema (high level)

The initial migration creates these tables:

- **`InterviewSlots`**
- **`StudentAvailability`**
- **`ScheduledInterview`** (FK to `InterviewSlots`)

## API endpoints

Currently implemented controller(s):

### Student availability

Base route:

- `api/students/{studentId}/availability`

Endpoints:

- **GET** `api/students/{studentId}/availability?date=YYYY-MM-DD`
  - Returns availability records for a student (optionally filtered by date).
- **POST** `api/students/{studentId}/availability`
  - Creates an availability record.
- **PUT** `api/students/{studentId}/availability/{id}`
  - Updates an availability record by id (and ensures it belongs to the student).
- **DELETE** `api/students/{studentId}/availability/{id}`
  - Deletes an availability record by id (scoped to the student).
- **POST** `api/students/{studentId}/availability/bulk`
  - Bulk-updates availability status for a date range.
- **POST** `api/students/{studentId}/availability/day/{date}/status?status=...&reason=...`
  - Updates the status for the whole day.

Example create request body:

```json
{
  "startTime": "2026-03-18T09:00:00",
  "endTime": "2026-03-18T10:00:00",
  "status": 0,
  "reasonStudent": "Can do morning"
}
```

Notes:

- `studentId` can be omitted from the body (or set to 0). If provided, it must match the route.
- `status` is an enum (`AvailabilityStatus`). Use the numeric value that matches your enum definition.

## Common commands

```powershell
# Build
dotnet build -c Release

# Run
dotnet run --project .\SchedulingService.csproj

# Add a migration (after model changes)
dotnet ef migrations add <Name> --project .\SchedulingService.csproj

# Update database to latest migration
dotnet ef database update --project .\SchedulingService.csproj
```

## Troubleshooting

- **Startup fails with missing `SCHEDULING_DB_CS`**
  - Set the env var in the same terminal where you run `dotnet run`.
- **SQL Server connection issues**
  - Verify the server name, authentication method, and that the database user has permissions to create/update tables.
- **EF tools not found**
  - This project references `Microsoft.EntityFrameworkCore.Tools`, so `dotnet ef` should work after restore. If it still fails, run `dotnet restore` and try again.
