# External Integrations

**Analysis Date:** 2026-04-13

## APIs & External Services

**Game API Endpoints:**
- Internal REST API only - No external third-party APIs detected
- Endpoints defined in `ImpoJuego.Api/Controllers/`
  - `/api/auth` - User registration and login
  - `/api/categories` - Word category management
  - `/api/game` - Game state and flow management
  - `/api/menu` - Game menu actions

**Third-Party Integrations:**
- None detected - This is a self-contained multiplayer game

## Data Storage

**Database:**
- SQLite (local file-based)
  - Location: `C:\_Mateo\_Mateo\ImpoJuego\impojuego\ImpoJuego.Api\impojuego.db`
  - Connection string: `Data Source=impojuego.db` (configured in `appsettings.json`)
  - Provider: Microsoft.EntityFrameworkCore.Sqlite 8.0.0
  - ORM: Entity Framework Core 8.0.0

**Database Seeding:**
- File: `ImpoJuego.Api/Data/DbSeeder.cs`
- Initialization: Runs automatically on application startup via `app.Services.CreateScope()` pattern
- Seeds: System categories (Spanish word categories) and default user data

**File Storage:**
- Local filesystem only - No cloud storage integration
- No file upload endpoints detected

**Caching:**
- None - No explicit caching layer (Redis, Memcached, etc.)
- In-memory session management via `GameSessionManager` singleton

## Authentication & Identity

**Auth Provider:**
- Custom JWT-based authentication
  - Implementation: `ImpoJuego.Api/Services/AuthService.cs`
  - Token type: Bearer JWT
  - Header: `Authorization: Bearer {token}`

**JWT Configuration (appsettings.json):**
- Secret: `ImpoJuegoSuperSecretKey2024ImpostorGameSecurityToken!@#$%` (hardcoded - SECURITY CONCERN)
- Issuer: `ImpoJuego`
- Audience: `ImpoJuegoApp`
- Expiration: 1440 minutes (24 hours)

**Token Validation:**
- Validates issuer, audience, lifetime, and signing key
- Configured in `Program.cs` lines 44-56
- Bearer token middleware added to HTTP pipeline

**User Registration/Login:**
- `POST /api/auth/register` - Email and password required
- `POST /api/auth/login` - Email and password
- Password hashing: BCrypt.Net-Next 4.0.3
- Frontend stores token and user info in localStorage (keys: `impojuego_token`, `impojuego_user`)

**Frontend Auth Integration:**
- Auth interceptor: `src/app/interceptors/auth.interceptor.ts` - Adds Bearer token to all requests
- Auth service: `src/app/services/auth.service.ts`
- Hardcoded API URL for auth: `https://impojuego-1.onrender.com/api` (production), `http://localhost:5000/api` (dev, commented)

## Monitoring & Observability

**Error Tracking:**
- None detected - No Sentry, LogRocket, or similar integration

**Logging:**
- Backend: Built-in ASP.NET Core logging
  - Configuration: `appsettings.json` sets log level to `Information` (default), suppresses `Microsoft.AspNetCore` to `Warning`
  - Console output in production
  - No persistent log storage detected

- Frontend: Angular logs to console only (development)
  - No structured logging framework integrated
  - No analytics or error reporting

**Performance Monitoring:**
- None detected

## CI/CD & Deployment

**Hosting:**
- Backend: Render.com at `https://impojuego-1.onrender.com` (API root with Swagger UI)
- Frontend: Render.com at `https://impojuego-web.onrender.com` (static build)
- Both use free-tier deployment (expected cold starts)

**Deployment Method:**
- Backend: Docker containerization
  - Dockerfile: `impojuego/Dockerfile`
  - Multi-stage build: SDK 8.0 -> AspNet 8.0 runtime
  - Exposed port: 5000
  - Render detects Dockerfile and builds/deploys accordingly

- Frontend: Standard Angular build
  - Build command: `npm run build` produces `dist/impojuego-web`
  - Deployed as static files to Render

**CI Pipeline:**
- None detected - No GitHub Actions, Azure DevOps, or Jenkins configuration
- Likely manual or Render's native build system

## Environment Configuration

**Required Environment Variables:**
- Backend (from code inspection, no explicit env var bindings found):
  - `JwtSettings:Secret` - Should be externalized to environment (currently hardcoded in appsettings.json)
  - `ConnectionStrings:DefaultConnection` - SQLite path (currently hardcoded to `impojuego.db`)

- Frontend:
  - No environment variables detected - API URL is hardcoded in source code (`app.config.ts`)

**Configuration Files Location:**
- Backend: `impojuego/ImpoJuego.Api/appsettings.json` and `appsettings.Development.json`
- Frontend: `impojuego-web/src/app/app.config.ts` (hardcoded config, no environment file pattern)

**Secrets Location:**
- Production: Likely Render.com environment variables (not shown in source)
- Development: appsettings.json (contains sensitive JWT secret)

## CORS & Cross-Origin Configuration

**CORS Policy: "Angular"**
- Configured in `Program.cs` lines 98-112
- Allowed origins:
  - `http://localhost:4200` (Angular dev server)
  - `http://localhost:5173` (Vite dev server, alternative)
  - `http://127.0.0.1:4200` (Localhost IPv4)
  - `https://impojuego-web.onrender.com` (Render production)

- Headers:
  - All headers allowed (`AllowAnyHeader()`)
  - Exposed header: `X-Session-Id` (for session management)
  - All methods allowed (`AllowAnyMethod()`)
  - Credentials allowed (`AllowCredentials()`)

**CORS Middleware:**
- Applied in `app.UseCors("Angular")` before authentication/authorization
- Order critical: Must come before `UseAuthentication()`

## Webhooks & Callbacks

**Incoming:**
- None detected - API is purely request-response REST

**Outgoing:**
- None detected - No outbound webhook integrations

## Session Management

**Session Identification:**
- Header-based: `X-Session-Id` (custom header)
- Each browser/client gets a unique session
- Frontend interceptor: `src/app/interceptors/session.interceptor.ts` adds header to all requests
- Backend manager: `GameSessionManager` (singleton) handles multiple concurrent sessions

**Session Lifecycle:**
- Stored in `GameSessionManager` (in-memory)
- Expiration: 4 hours of inactivity (hardcoded in `Program.cs`: `TimeSpan.FromHours(4)`)
- Cleanup: Auto-cleanup every 30 minutes
- Session data includes all game state (players, votes, roles, etc.)

**Frontend Session Persistence:**
- Session ID stored in localStorage (via `SessionService`)
- Auth token also stored in localStorage
- Survives page refresh

## API Response Format

**Wrapper:**
- All responses wrapped in `ApiResponse<T>` class
- Structure: `{ success: bool, message: string, data: T }`
- Consistent across all endpoints

**Status Codes:**
- 200 OK for successful operations
- 401 Unauthorized for auth failures
- 403 Forbidden for unauthorized access
- 400 Bad Request for validation errors
- 500 Internal Server Error for unhandled exceptions

## Database Schema

**Entities:**
- `User` - Email (unique), PasswordHash, Role (Admin/User)
- `Category` - Name, Owner (User foreign key), Words (one-to-many)
- `Word` - Text, Category (Category foreign key)

**Relationships:**
- User → Category (one-to-many, cascade delete)
- Category → Word (one-to-many, cascade delete)

**Constraints:**
- Email unique and required (max 255 chars)
- Category name required (max 100 chars)
- Word text required (max 200 chars)
- All foreign keys with cascade delete behavior

## Integration Issues & Concerns

1. **JWT Secret Exposure**
   - Hardcoded secret in `appsettings.json` and fallback in `Program.cs`
   - Production should use Render environment variables
   - Secret visible in git history if committed

2. **API URL Hardcoding**
   - Frontend API URL hardcoded in `src/app/app.config.ts`
   - Development requires code change to switch between localhost and production
   - No environment-based configuration pattern

3. **SQLite in Cloud**
   - File-based SQLite not ideal for Render deployment
   - May have issues with concurrent access or persistence
   - Consider PostgreSQL for production

4. **No Error Reporting**
   - Production errors only logged to console
   - No way to detect and respond to production issues
   - Consider Sentry or similar for error tracking

5. **Session State Volatility**
   - Game state stored in-memory only (GameSessionManager)
   - Lost on application restart or redeployment
   - Active games interrupted during Render cold starts

6. **CORS Credentials**
   - `AllowCredentials()` enabled with `*` origins would be invalid, but specific origins listed
   - Current configuration is secure, but `.WithOrigins()` does not include backend domain

---

*Integration audit: 2026-04-13*
