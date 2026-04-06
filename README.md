# TouristParkingManageSystem

This project is a web application project of Tourist Parking Management System (TPMS) for a National Park.

It is based on C# .NET and a React PWA web stack, and includes:

- Clean Architecture + DDD modular monolith with separate edge-node service
- CQRS handlers with MediatR and FluentValidation
- EF Core persistence with optimistic concurrency via `RowVersion`
- SignalR live updates for bay occupancy and reservation changes
- Serilog structured JSON logging and OpenTelemetry tracing
- React PWA frontend for tourists, controllers, and admin users
- Docker packaging, local compose orchestration, and GitHub Actions CI/CD

## Projects

- `src/TPMS.Domain`: domain aggregates, value objects, events
- `src/TPMS.Application`: CQRS, DTOs, abstractions, behaviors
- `src/TPMS.Infrastructure`: EF Core, repositories, Stripe, outbox, seeding
- `src/TPMS.Api`: ASP.NET Core Web API, auth, SignalR, static hosting
- `src/TPMS.Edge`: local edge-node API and offline sync worker
- `src/TPMS.Web`: React + TypeScript PWA

## Local Development

1. Restore backend packages: `dotnet restore TPMS.slnx`
2. Install frontend packages: `cd src/TPMS.Web && npm install`
3. Run the API: `dotnet run --project src/TPMS.Api`
4. Run the edge node: `dotnet run --project src/TPMS.Edge`
5. Run the frontend: `cd src/TPMS.Web && npm run dev`

The seeded local admin account is `admin@tpms.local` with password `Admin123!`.

## Containers

- Central cloud service: `Dockerfile.api`
- Edge-node service: `Dockerfile.edge`
- Local stack: `docker compose up --build`


