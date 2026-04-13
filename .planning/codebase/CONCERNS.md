# Codebase Concerns

**Analysis Date:** 2026-04-13

## 🔴 CRITICAL: Deploy/Build Broken

### 1. **Missing PORT Environment Variable Support (Render Deployment BLOCKER)**
- **Issue:** ASP.NET Core app hardcodes port 5000 in `launchSettings.json`. Render assigns dynamic PORT via env var. App will fail to start on Render because it cannot bind to the assigned port.
- **Files:** 
  - `impojuego/ImpoJuego.Api/Properties/launchSettings.json` (line 8: hardcoded localhost:5000)
  - `impojuego/ImpoJuego.Api/Program.cs` (no env var reading for port)
- **Impact:** **Deploy completely broken.** Backend service fails to start on Render.
- **Fix approach:** Add `builder.WebApplication.CreateBuilder(args)` environment variable reading for PORT. Must use `builder.Configuration["PORT"]` and configure `Kestrel` to bind to dynamic port. Example:
  ```csharp
  var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
  builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
  ```
- **Priority:** CRITICAL - must fix before any deploy will work

### 2. **SQLite Database File at Project Root (Ephemeral Filesystem)**
- **Issue:** Connection string uses relative path `Data Source=impojuego.db`. Render uses ephemeral filesystem that resets on redeploy. Database persists only during single instance lifecycle, then is lost.
- **Files:**
  - `impojuego/ImpoJuego.Api/appsettings.json` (line 16: `Data Source=impojuego.db`)
- **Impact:** All user data (auth, categories) is lost on each redeploy. Users cannot log back in. Game state resets.
- **Fix approach:** 
  - Option A (Quick): Use `/tmp/` directory (available on Render, lasts deploy duration): `Data Source=/tmp/impojuego.db`
  - Option B (Production): Migrate to persistent database (PostgreSQL on Render, change EF Core provider to `Microsoft.EntityFrameworkCore.Npgsql`)
  - For now, at minimum change to: `Data Source=${HOME}/impojuego.db` if HOME env var available, or hardcode `/tmp/`
- **Priority:** CRITICAL - data loss on every deploy

### 3. **Frontend API URL Must Match Backend (Missing Production Protocol)**
- **Issue:** Frontend hardcodes multiple API URLs but they're inconsistent across services. All point to `https://impojuego-1.onrender.com/api` which must match actual backend domain. If backend domain changes or is mistyped, all frontend API calls fail.
- **Files:**
  - `impojuego-web/src/app/app.config.ts` (line 16: `apiUrl: 'https://impojuego-1.onrender.com/api/game'`)
  - `impojuego-web/src/app/services/auth.service.ts` (line 10: hardcoded `https://impojuego-1.onrender.com/api`)
  - `impojuego-web/src/app/services/categories.service.ts` (multiple hardcoded URLs)
- **Impact:** If backend Render URL changes, frontend breaks silently. CORS errors in production, blank page to users.
- **Fix approach:** Centralize API URL config. Use environment-based config or read from a single source of truth. Consider:
  - Create `src/app/config/api.config.ts` with single source
  - Or use Angular environment files (`environment.prod.ts`)
  - Or read from backend `/api/config` endpoint at startup
- **Priority:** HIGH - prevents easy redeployment/URL changes

### 4. **No Render Deployment Configuration (render.yaml Missing)**
- **Issue:** No `render.yaml` or deployment instructions. Render doesn't know:
  - How to build Angular frontend separately
  - Where static files live (`dist/impojuego-web/browser/`)
  - Which service to expose on which port
  - Environment variables needed
  - Database migration steps
- **Files:** No render.yaml found in repo root
- **Impact:** Manual Render deployment setup required. Easy to misconfigure. Different from infrastructure-as-code best practice.
- **Fix approach:** Create `render.yaml` with:
  ```yaml
  services:
    - type: web
      name: impojuego-api
      runtime: dotnet
      buildCommand: "cd impojuego && dotnet publish -c Release"
      startCommand: "cd impojuego && dotnet ImpoJuego.Api.dll"
      envVars:
        - key: ASPNETCORE_ENVIRONMENT
          value: Production
    - type: static_site
      name: impojuego-web
      staticPublishPath: ./impojuego-web/dist/impojuego-web/browser
      buildCommand: "cd impojuego-web && npm ci && npm run build"
  ```
- **Priority:** HIGH - ensures reproducible deployments

### 5. **No SPA Fallback Server Configuration for Frontend**
- **Issue:** Frontend is SPA (Angular). Static file servers must serve `index.html` for all unknown routes (for client-side routing). Render's static site hosting may not be configured for this.
- **Files:**
  - `impojuego-web/dist/impojuego-web/browser/index.html` exists
  - `impojuego-web/src/app/app.routes.ts` (has SPA routes with wildcard fallback)
- **Impact:** Direct links to `/game`, `/voting`, `/result` fail with 404. Users must enter through root `/` only.
- **Fix approach:** Render static site needs rewrite rule: "Rewrite all requests to `index.html` if file not found"
  Or use web server wrapper. Check Render static site documentation for rewrite configuration.
- **Priority:** HIGH - breaks deep linking

---

## 🟡 Bugs & Logic Issues

### 6. **JWT Secret Hardcoded in Source Code**
- **Issue:** JWT secret stored in `appsettings.json` committed to git. Anyone with repo access has the secret key. Production tokens can be forged.
- **Files:**
  - `impojuego/ImpoJuego.Api/appsettings.json` (line 10: `"Secret": "ImpoJuegoSuperSecretKey2024ImpostorGameSecurityToken!@#$%"`)
- **Impact:** Medium security risk. Token can be forged offline. 
- **Current mitigation:** Line 35 in `Program.cs` has fallback: `?? new JwtSettings { Secret = "DefaultSecretKeyForDevelopment12345678!" }` - but this is also hardcoded.
- **Fix approach:** 
  - Remove from `appsettings.json` (keep placeholder)
  - Use Render environment variables: `JWTSETTINGS_SECRET` 
  - Read via: `builder.Configuration.GetSection("JwtSettings")` (already does this, just need env vars set on Render)
  - Fallback should error, not use default
- **Priority:** MEDIUM - production secret leaked

### 7. **Database Creation via EnsureCreatedAsync (No Migrations)**
- **Issue:** `DbSeeder.cs` calls `context.Database.EnsureCreatedAsync()` which creates schema directly without EF Core migrations. This:
  - Bypasses migration history
  - Makes schema changes manual
  - Can't roll back schema changes
  - Doesn't work well with schema versioning
- **Files:**
  - `impojuego/ImpoJuego.Api/Data/DbSeeder.cs` (line 11)
  - `impojuego/ImpoJuego.Api/Program.cs` (line 121: calls seeder)
- **Impact:** Schema changes require manual DB updates. No migration rollback capability. Hard to track schema version.
- **Fix approach:** Implement proper EF Core migrations:
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```
  Update startup to run migrations: `context.Database.Migrate();` instead of `EnsureCreatedAsync`
- **Priority:** MEDIUM - limits schema management

### 8. **Admin User Hardcoded in DbSeeder**
- **Issue:** Admin account credentials hardcoded: `email: "mateocirujas"`, `password: "mateo"`. Anyone with code access knows prod admin password.
- **Files:**
  - `impojuego/ImpoJuego.Api/Data/DbSeeder.cs` (lines 22, 28)
- **Impact:** Production admin account compromise if anyone accesses code or DB.
- **Fix approach:** 
  - Read from env vars: `ADMIN_EMAIL`, `ADMIN_PASSWORD`
  - Only seed if env vars present (don't hardcode fallback)
  - Or remove admin seed entirely, create via separate admin setup script
- **Priority:** MEDIUM - admin creds exposed

---

## 🟠 Security Concerns

### 9. **CORS Configuration Allows Any Method**
- **Issue:** `AllowAnyMethod()` in CORS policy without explicit method list. Allows DELETE, PATCH, OPTIONS, etc. on all endpoints.
- **Files:**
  - `impojuego/ImpoJuego.Api/Program.cs` (line 110: `AllowAnyMethod()`)
- **Impact:** Unintended HTTP methods can be called on protected resources. DELETE requests not normally exposed to frontend could be exploited by CORS bypass.
- **Fix approach:** Be explicit:
  ```csharp
  .WithMethods("GET", "POST", "OPTIONS")
  ```
- **Priority:** LOW - frontend controls requests, but defense-in-depth recommended

### 10. **JWT Token Validation Missing Audience/Issuer Check**
- **Issue:** Token validation requires audience and issuer (lines 48-49 in Program.cs), but they're hardcoded generic values. No validation that tokens are specifically for this app.
- **Files:**
  - `impojuego/ImpoJuego.Api/Program.cs` (lines 48-54)
  - `impojuego/ImpoJuego.Api/appsettings.json` (lines 11-12: hardcoded "ImpoJuego")
- **Impact:** Tokens from other systems with same issuer/audience would validate. Low risk but not production-hardened.
- **Fix approach:** Use specific audience/issuer or skip if same across all systems. Already doing this correctly, just note it's not customized per deployment.
- **Priority:** LOW - already implemented

### 11. **Password Storage Uses BCrypt (Good) But No Salt Validation**
- **Issue:** `BCrypt.Net.BCrypt.HashPassword()` is used (good), but:
  - No minimum password length check
  - No password complexity requirements
  - "mateo" as admin password is weak
- **Files:**
  - `impojuego/ImpoJuego.Api/Data/DbSeeder.cs` (line 28: hashes "mateo")
  - `impojuego/ImpoJuego.Api/Services/AuthService.cs` (likely hashes user passwords)
- **Impact:** Weak passwords accepted. Brute force risk on "mateocirujas" admin account.
- **Fix approach:** Add validation in `RegisterAsync`:
  ```csharp
  if (password.Length < 8) return (null, "Password must be 8+ characters");
  ```
- **Priority:** LOW - auth endpoints not exposed to public game

---

## 🟣 Tech Debt

### 12. **No Environment-Specific Configuration**
- **Issue:** Uses single `appsettings.json`. No `appsettings.Production.json`. All config same for dev and prod.
- **Files:**
  - `impojuego/ImpoJuego.Api/appsettings.json` (production secrets)
  - `impojuego/ImpoJuego.Api/appsettings.Development.json` (empty, unused)
- **Impact:** Easy to commit dev secrets to prod. No prod-specific tuning (logging level, database pool size, etc.).
- **Fix approach:** Create `appsettings.Production.json`:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "JwtSettings": {
      "ExpirationMinutes": 1440
    }
  }
  ```
  Set `ASPNETCORE_ENVIRONMENT=Production` on Render.
- **Priority:** MEDIUM - better practice

### 13. **API Response Wrapper Inconsistency**
- **Issue:** All endpoints return `ApiResponse<T>` wrapper, but error responses use `BadRequest(new ApiResponse<object>(false, error, null))`. Frontend must check both `response.data` and `response.success`. No TypeScript type safety for error responses.
- **Files:**
  - `impojuego/ImpoJuego.Api/Controllers/AuthController.cs` (lines 25-26: error handling)
  - `impojuego/ImpoJuego.Api/Controllers/CategoriesController.cs` (multiple error returns)
- **Impact:** Frontend must handle both `.data` (success) and direct error messages. Inconsistent error structure.
- **Fix approach:** Define consistent error response:
  ```csharp
  public record ApiResponse<T>(bool Success, string Message, T? Data);
  // Always return same structure, even on error
  return Ok(new ApiResponse<string>(false, "Validation error", null));
  ```
- **Priority:** LOW - works but not clean

### 14. **No Logging Infrastructure**
- **Issue:** No structured logging (Serilog). Only uses default `Console.WriteLine()` for startup info. No request logging, error logging, or audit trail.
- **Files:**
  - `impojuego/ImpoJuego.Api/Program.cs` (lines 142-146: manual console.writelines)
  - No Serilog or logging middleware
- **Impact:** Hard to debug issues on Render. No audit trail for security issues. Can't correlate requests.
- **Fix approach:** Add Serilog:
  ```bash
  dotnet add package Serilog.AspNetCore
  ```
  Configure in Program.cs with file/console sinks.
- **Priority:** LOW - nice-to-have for production

### 15. **Frontend TypeScript Types May Be Incomplete**
- **Issue:** Angular services map API responses to TypeScript models. If backend changes response shape, frontend doesn't catch it at compile time (no schema validation).
- **Files:**
  - `impojuego-web/src/app/models/` (types defined)
  - `impojuego-web/src/app/services/*.service.ts` (map API to models)
- **Impact:** Breaking API changes silently fail at runtime in frontend.
- **Fix approach:** Consider OpenAPI/Swagger integration with code generation (NSwag) to auto-generate TypeScript types from C# models.
- **Priority:** LOW - works but could be safer

---

## 🔵 Performance & Scalability

### 16. **Session Storage In-Memory (Render Ephemeral)**
- **Issue:** `GameSessionManager` uses `Dictionary<string, GameInstance>` held in memory. Sessions are lost on redeploy or scaling to multiple instances.
- **Files:**
  - Not shown in excerpts, but mentioned in CLAUDE.md: sessions stored in-memory
- **Impact:** If Render scales to multiple instances, session affinity required (sticky sessions). Games in progress lost on redeploy.
- **Fix approach:** Migrate sessions to Redis or distributed cache:
  - Add `StackExchange.Redis` package
  - Store session state in Redis
  - Or implement session persistence to SQLite (but then DB must persist)
- **Priority:** MEDIUM - limits scaling

### 17. **No Database Connection Pooling Configuration**
- **Issue:** SQLite connection string has no pool size configuration. Default pool may be too small under load.
- **Files:**
  - `impojuego/ImpoJuego.Api/Program.cs` (line 31: no pool config)
- **Impact:** High concurrent users may exhaust connection pool.
- **Fix approach:** Add to connection string: `;Max Pool Size=10;`
- **Priority:** LOW - game has small player count

---

## 🟠 Fragile Areas (High Maintenance Risk)

### 18. **GameManager State Machine Not Thread-Safe**
- **Issue:** `GameManager` orchestrates game phases but is not documented as thread-safe. If simultaneous requests modify game state, race conditions possible.
- **Files:**
  - `impojuego/impojuego/Managers/GameManager.cs` (265 lines, complex state transitions)
- **Impact:** Concurrent players might trigger invalid state transitions (e.g., vote during non-voting phase).
- **Fix approach:** 
  - Add `lock` statements in GameManager methods, or
  - Use `Interlocked` operations for state flags, or
  - Document required session locking at controller level
  - Add comments: "This method is not thread-safe. Call only within GameManager critical section."
- **Priority:** MEDIUM - only breaks with concurrent players in same session

### 19. **Frontend Polling Without Backoff**
- **Issue:** `GameStateService` likely polls `/api/game/state` at fixed interval. If server slow, requests queue up.
- **Files:**
  - `impojuego-web/src/app/services/game.service.ts` (may call from component with setInterval)
  - Not shown: GameStateService implementation
- **Impact:** High server load if polling interval too fast or server unresponsive.
- **Fix approach:** Implement exponential backoff in polling interceptor. Or use WebSocket instead of HTTP polling.
- **Priority:** LOW - for game with few players

### 20. **WordCategories Data Hardcoded and Large**
- **Issue:** `DbSeeder.cs` has ~150 words across 7 categories hardcoded. Difficult to update without code change.
- **Files:**
  - `impojuego/ImpoJuego.Api/Data/DbSeeder.cs` (lines 47-137: huge dictionary)
- **Impact:** Adding/removing words requires code deploy. Categories can't be modified by admin at runtime (only added via `/api/categories` endpoint).
- **Fix approach:** Load from JSON file instead:
  ```csharp
  var words = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
    File.ReadAllText("Data/defaultCategories.json")
  );
  ```
  Or keep in DB, seed only on first run (not every startup).
- **Priority:** LOW - game works, just awkward

---

## Test Coverage

### 21. **Frontend Has No Tests**
- **Issue:** Only backend has tests (145 tests, 100% coverage). Frontend components untested.
- **Files:**
  - `impojuego/ImpoJuego.Tests/` has 145 tests
  - `impojuego-web/` has test files commented out (skipTests: true in angular.json)
- **Impact:** Frontend refactors may break UI. Component logic errors not caught.
- **Fix approach:** 
  - Enable tests: remove `skipTests: true` from angular.json
  - Add Jasmine/Karma tests for services and components
  - Aim for 70%+ coverage on GameService, AuthService
- **Priority:** LOW - game is simple, few complex interactions

---

## Missing Critical Features

### 22. **No Health Check Endpoint for Render**
- **Issue:** Render expects `/health` or similar endpoint to determine if app is alive. Currently only Swagger at root.
- **Files:**
  - `impojuego/ImpoJuego.Api/Program.cs` (no health endpoint mapped)
- **Impact:** Render may not properly detect when app fails to start. Deployment appears to succeed but app is down.
- **Fix approach:** Add simple health check:
  ```csharp
  app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
  ```
  Configure Render to check `/health` endpoint.
- **Priority:** MEDIUM - helps Render monitoring

---

## Summary: Top 5 Showstoppers

1. **Missing PORT env var support** - App won't start on Render at all
2. **SQLite at project root** - All data lost on redeploy (ephemeral filesystem)
3. **Hardcoded frontend API URLs** - Frontend breaks if backend URL changes
4. **No render.yaml** - Deployment requires manual Render setup
5. **No SPA fallback configuration** - Deep links to routes (e.g., `/voting`) fail with 404

These 5 issues completely break the deployment. Fix these before anything else works.

---

*Concerns audit: 2026-04-13*
