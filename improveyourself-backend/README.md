# ImproveYourself Backend

This is the backend API for the ImproveYourself mobile app, built with .NET 10, EF Core, and PostgreSQL.

## Features

- **Daily Challenges**: Manage and sync daily challenges.
- **Quotes**: Daily quote content.
- **Progress Sync**: Idempotent synchronization for offline-first clients.
- **Statistics**: Basic history and statistics.
- **Analytics**: Logging for client events.

## Architecture

The project follows Clean Architecture principles:

- **Domain**: Entities, enums, and core logic.
- **Application**: Interfaces, DTOs, services, and business logic.
- **Infrastructure**: Persistence (EF Core, PostgreSQL) and external services.
- **Api**: ASP.NET Core Web API controllers and middleware.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (for PostgreSQL)

### Running with Docker (PostgreSQL)

1. Start a PostgreSQL container:
   ```bash
   docker run --name improveyourself-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=improveyourself -p 5432:5432 -d postgres
   ```

2. Update `appsettings.json` with your connection string if necessary.

### Running the API

1. Navigate to the API project directory:
   ```bash
   cd ImproveYourself.Backend.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

### Database Migrations

To apply migrations to the database:

```bash
dotnet ef database update --project ../ImproveYourself.Backend.Infrastructure --startup-project .
```

## API Endpoints

- `GET /api/challenges`: Get daily challenges (optional `startDate`, `endDate` query params).
- `POST /api/challenges/sync`: Sync local challenges to the backend (Idempotent).
- `GET /api/quotes/daily`: Get the quote of the day.
- `POST /api/analytics`: Log an analytics event (Idempotent).
- `GET /api/stats`: Get user progress statistics (requires `clientId`).

## Connecting the Mobile App

To connect the ImproveYourself MAUI app to this backend:

1. Update the base API URL in the mobile app's configuration.
2. Implement a background synchronization service in the mobile app that calls `POST /api/challenges/sync`.
3. Use `Guid` for all new entities created on the client to ensure unique identification during sync.
4. Use the `UpdatedAt` timestamp to resolve conflicts (server-side logic already uses this for "last-writer-wins" strategy).
