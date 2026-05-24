# dotnet-cqrs-template

A .NET 10 CQRS template with ASP.NET Core minimal APIs, EF Core + Postgres, MediatR, FluentValidation, Mapperly, and Serilog.

## Install the Template

**Install locally from this repo:**
```
dotnet new install ./
```

**Scaffold a new project:**
```
dotnet new cqrs-api --name MyProject
```

**Uninstall:**
```
dotnet new uninstall ./
```

---

## Prerequisites

- .NET 10 SDK
- Docker (for Postgres)

## Setup

**1. Start Postgres:**
```
docker-compose up -d
```

**2. Set the connection string (stored in user-secrets, never committed):**
```
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=backendtemplate;Username=postgres;Password=postgres" --project backend/src/BackendTemplate.Api
```

**3. Apply migrations:**
```
dotnet ef database update --project backend/src/BackendTemplate.Infrastructure --startup-project backend/src/BackendTemplate.Api
```

**4. Run the API:**
```
dotnet watch --project backend/src/BackendTemplate.Api
```

OpenAPI spec: `http://localhost:5000/openapi/v1.json`  
Scalar UI (dev only): `http://localhost:5000/scalar/v1`  
Health check: `http://localhost:5000/health`

## Development

**Build:**
```
dotnet build backend/BackendTemplate.slnx
```

**Test:**
```
dotnet test backend/BackendTemplate.slnx
```

**Add migration:**
```
dotnet ef migrations add <Name> --project backend/src/BackendTemplate.Infrastructure --startup-project backend/src/BackendTemplate.Api
```
