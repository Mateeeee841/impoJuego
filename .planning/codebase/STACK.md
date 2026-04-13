# Technology Stack

**Analysis Date:** 2026-04-13

## Languages

**Primary:**
- C# .NET 8 - Backend API and core game logic
- TypeScript 5.4.2 - Angular 17 frontend
- HTML5 / SCSS - Template and styling

**Secondary:**
- Bash/PowerShell - Docker build scripts

## Runtime

**Environment:**
- .NET 8 (mcr.microsoft.com/dotnet/sdk:8.0 for build, mcr.microsoft.com/dotnet/aspnet:8.0 for runtime)
- Node.js (version not pinned; Angular CLI requires modern Node)

**Package Manager:**
- NuGet (implicit in .NET SDK)
- npm (Angular project dependency manager)
- Lockfile: `package-lock.json` present in `impojuego-web/`

## Frameworks

**Core Backend:**
- ASP.NET Core 8 Web API - REST API framework
- Entity Framework Core 8.0.0 - ORM for SQLite database
- Swagger/Swashbuckle 6.5.0 - API documentation and testing UI

**Frontend:**
- Angular 17.3.0 - Main framework
- @angular/router - Client-side routing
- @angular/forms - Form handling
- @angular/common - Common utilities

**Authentication:**
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0 - JWT validation
- BCrypt.Net-Next 4.0.3 - Password hashing

**Testing Backend:**
- xUnit 2.9.2 - Test framework
- FluentAssertions 8.8.0 - Assertion library
- Moq 4.20.72 - Mocking framework
- Microsoft.AspNetCore.Mvc.Testing 8.0.0 - Integration testing
- Coverlet 6.0.4 - Code coverage (msbuild and collector)

**Testing Frontend:**
- Karma 6.4.0 - Test runner
- Jasmine 5.1.0 - Test framework and assertions
- karma-jasmine - Karma adapter for Jasmine
- karma-chrome-launcher - Chrome browser launcher for tests
- karma-coverage 2.2.0 - Coverage reporter

**Build/Dev Tools:**
- @angular/cli 17.3.5 - Angular development CLI
- @angular-devkit/build-angular 17.3.5 - Angular build system
- TypeScript 5.4.2 - Type checking

## Key Dependencies

**Critical:**
- Microsoft.EntityFrameworkCore.Sqlite 8.0.0 - Database connectivity (SQLite provider)
- Microsoft.EntityFrameworkCore.Design 8.0.0 - EF Core tooling
- @angular/common/http - HTTP client for API calls (implicit with Angular)
- rxjs 7.8.0 - Reactive programming library

**Infrastructure:**
- BCrypt.Net-Next 4.0.3 - Secure password hashing
- Swashbuckle.AspNetCore 6.5.0 - Swagger/OpenAPI support
- zone.js 0.14.3 - Angular zones polyfill
- tslib 2.3.0 - TypeScript helper library

## Configuration

**Environment:**
- Backend: Environment variables via `appsettings.json` and `appsettings.Development.json`
  - `JwtSettings:Secret` - JWT signing key (hardcoded in development, should use env vars in production)
  - `JwtSettings:Issuer` - Token issuer claim
  - `JwtSettings:Audience` - Token audience claim
  - `JwtSettings:ExpirationMinutes` - Token lifetime (default 1440 = 24 hours)
  - `ConnectionStrings:DefaultConnection` - SQLite database path (default: `impojuego.db`)
  - `Logging:LogLevel` - Logging configuration

- Frontend: Configuration in `src/app/app.config.ts`
  - `CONFIG.apiUrl` - Production: `https://impojuego-1.onrender.com/api/game`; Dev: `http://localhost:5000/api/game` (commented)

**Build:**
- Backend: Multi-stage Dockerfile (`impojuego/Dockerfile`)
  - Build stage: SDK 8.0 -> `dotnet restore` and `dotnet publish`
  - Runtime stage: AspNet 8.0 runtime
  - Exposed port: 5000
  - Entrypoint: `ImpoJuego.Api.dll`

- Frontend:
  - `angular.json` - Angular build and serve configuration
  - Output path: `dist/impojuego-web`
  - Production budgets: 500KB initial bundle, 1MB max, 20KB per component style, 30KB max
  - `tsconfig.json` - TypeScript compilation options (ES2022 target, strict mode enabled)
  - `tsconfig.app.json` - App-specific TypeScript configuration
  - `tsconfig.spec.json` - Test-specific TypeScript configuration

## Dependency Versions Summary

**Backend (.NET 8):**
- Swashbuckle.AspNetCore 6.5.0
- Microsoft.EntityFrameworkCore.Sqlite 8.0.0
- Microsoft.EntityFrameworkCore.Design 8.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- BCrypt.Net-Next 4.0.3

**Test Dependencies (.NET 9 - MISMATCH ALERT):**
- coverlet.collector 6.0.4
- coverlet.msbuild 6.0.4
- FluentAssertions 8.8.0
- Microsoft.AspNetCore.Mvc.Testing 8.0.0
- Microsoft.NET.Test.Sdk 17.12.0
- Moq 4.20.72
- xunit 2.9.2
- xunit.runner.visualstudio 2.8.2

**Frontend (Node/npm):**
- @angular/* packages 17.3.0 / 17.3.5
- TypeScript 5.4.2
- rxjs 7.8.0
- karma 6.4.0
- jasmine-core 5.1.0

## Platform Requirements

**Development:**
- Visual Studio 2022 (v17.13+) or VS Code
- .NET 8 SDK installed
- Node.js (modern version, no pinned version specified)
- Docker (for containerization)
- SQLite 3 (implicit)

**Production:**
- Docker-compatible runtime environment
- Render cloud platform (currently deployed)
- HTTPS support for frontend-backend communication

## Notable Configuration Details

1. **JWT Secret Exposure**: Hardcoded JWT secret in `appsettings.json` (`ImpoJuegoSuperSecretKey2024ImpostorGameSecurityToken!@#$%`). Production should use environment variables or Render's secrets manager.

2. **Database**: SQLite file at `impojuego.db` in working directory. Works for development/deployment but should consider path configuration for cloud deployments.

3. **CORS**: Configured for localhost dev (4200, 5173, 127.0.0.1) and Render production (`https://impojuego-web.onrender.com`). Does not expose origin for backend.

4. **Target Framework Mismatch**: Test project targets `net9.0` while other projects target `net8.0`. This should be aligned.

---

*Stack analysis: 2026-04-13*
