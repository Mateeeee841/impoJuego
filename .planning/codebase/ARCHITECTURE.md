# Architecture

**Analysis Date:** 2026-04-13

## Pattern Overview

**Overall:** Layered architecture with a multi-session game engine backend (Core → Managers → API Controllers) and a reactive Angular frontend with service-based state management.

**Key Characteristics:**
- **Backend:** Manager-based orchestration with session isolation via `X-Session-Id` header
- **Frontend:** Standalone Angular 17 components with centralized state management via `GameStateService`
- **Cross-cutting:** Session identity maintained through HTTP interceptors; polling-based state synchronization
- **Game Logic:** State machine progression through GamePhase enum (Lobby → RoleReveal → Discussion → Voting → Finished)

## Layers

**Core Game Logic (impojuego library):**
- Purpose: Pure game state and business logic, independent of HTTP or UI concerns
- Location: `/impojuego/impojuego/`
- Contains: Game models (`Player`, `GameSession`), managers (`GameManager`, `PlayerManager`, `VotingManager`, `MenuManager`), configurations (`GameSettings`)
- Depends on: Internal models and configuration only
- Used by: `ImpoJuego.Api` project; Console app for testing

**API Layer (ImpoJuego.Api):**
- Purpose: Expose game logic via REST endpoints; handle HTTP concerns (authentication, CORS, session headers)
- Location: `/impojuego/ImpoJuego.Api/`
- Contains: Controllers (`GameController`, `MenuController`, `AuthController`, `CategoriesController`), DTOs for API responses, services (`AuthService`, `CategoryService`), database context (`ImpoJuegoDbContext`)
- Depends on: Core game logic, Entity Framework, JWT authentication
- Used by: Angular frontend via HTTP client

**Frontend Layer (impojuego-web):**
- Purpose: UI for player interaction across all game phases
- Location: `/impojuego-web/src/app/`
- Contains: Standalone components (`LobbyComponent`, `GameComponent`, `VotingComponent`, `ResultComponent`, `LoginComponent`), services (`GameService`, `GameStateService`, `SessionService`, `AuthService`), interceptors, models/DTOs
- Depends on: Angular 17, RxJS for reactive state management
- Used by: End user browsers

## Data Flow

**Game Start Flow:**

1. Player registration: Frontend `LobbyComponent` → `GameService.registerPlayer()` → API `GameController.RegisterPlayer()` → Backend `GameManager.RegisterPlayer()` → stored in session `GameSession.Game.Players`
2. Game initialization: `GameService.startGame()` → `GameController.StartGame()` → `GameManager.StartGame()` sets phase to `RoleReveal`, selects category and word
3. Role reveal polling: Frontend polls `GameService.getGameState()` every 3 seconds, `GameStateService.startPolling()` triggers `GameStateService.navigateToCurrentPhase()` to route to `/game` component
4. Vote collection: `VotingComponent` calls `GameService.castVote()` → `GameController.Vote()` → `VotingManager.CastVote()` stores votes in session
5. Round result: `GameService.tallyVotes()` → `GameController.Tally()` → `GameManager.ProcessVotingResult()` eliminates players, updates phase
6. Game end: Phase transitions to `Finished`, frontend navigates to `/result`, shows `GameEnd` data from `GameService.getGameResult()`

**State Management:**

Backend:
- `GameSessionManager` maintains a `ConcurrentDictionary<string, GameSession>` keyed by session ID from `X-Session-Id` header
- Each `GameSession` contains immutable `SessionId`, `GameManager` instance, timestamps (`CreatedAt`, `LastAccessedAt`)
- `GameManager` is stateful: `CurrentPhase`, `CurrentCategory`, `CurrentWord`, `RoundNumber`, `Players` collection, `Voting` state
- State persists in memory for 4 hours; cleanup runs every 30 minutes

Frontend:
- `SessionService` manages persistent session ID in localStorage; reused across requests via `sessionInterceptor`
- `GameStateService` holds `BehaviorSubject<GameState | null>` as single source of truth
- Components subscribe to `GameStateService.getState()` Observable
- Polling via `interval(3000).pipe(switchMap(() => refreshState()))` keeps frontend in sync; stopped on game end
- Phase-based navigation: `navigateToCurrentPhase()` routes to `/lobby`, `/game`, `/voting`, or `/result` based on `GameState.phase`

## Key Abstractions

**GameManager:**
- Purpose: Orchestrates all game phases and player interactions
- Examples: `impojuego/impojuego/Managers/GameManager.cs`
- Pattern: Stateful manager with phase transition guards; methods check `CurrentPhase` before state changes (e.g., `RegisterPlayer` only works in `Lobby` phase)

**GameSessionManager:**
- Purpose: Multi-session support; isolation between concurrent game instances
- Examples: `impojuego/impojuego/Managers/GameSessionManager.cs`
- Pattern: Thread-safe `ConcurrentDictionary`; automatic cleanup of stale sessions via `Timer`

**SessionControllerBase:**
- Purpose: Shared session handling across all game controllers
- Examples: `ImpoJuego.Api/Controllers/SessionControllerBase.cs`
- Pattern: Template Method; `GetOrCreateSession()` and `GetGame()` extract X-Session-Id header and return scoped `GameManager`

**GameStateService:**
- Purpose: Single source of truth for frontend game state
- Examples: `impojuego-web/src/app/services/game-state.service.ts`
- Pattern: RxJS BehaviorSubject; polling loop with switchMap; localStorage for persistence

**sessionInterceptor & authInterceptor:**
- Purpose: Transparent header injection
- Examples: `impojuego-web/src/app/interceptors/session.interceptor.ts`, `auth.interceptor.ts`
- Pattern: Angular HttpInterceptorFn; clone request with setHeaders; no conditional logic in interceptors (conditional auth is in service)

## Entry Points

**Backend:**
- Location: `impojuego/ImpoJuego.Api/Program.cs`
- Triggers: `dotnet run` or `dotnet watch run`
- Responsibilities: 
  - Service registration (JWT auth, database, GameSessionManager, controllers)
  - Middleware setup (CORS, authentication, Swagger)
  - Database initialization via `DbSeeder.SeedDatabaseAsync()`
  - API listens on `http://localhost:5000`

**Console/Test Entry:**
- Location: `impojuego/impojuego/Program.cs`
- Triggers: `dotnet run` from impojuego project root
- Responsibilities: Manual gameplay loop; demonstrates all game phases without HTTP

**Frontend:**
- Location: `impojuego-web/src/main.ts` → `AppComponent`
- Triggers: `npm start` (runs `ng serve` on port 4200)
- Responsibilities: Bootstrap with `appConfig` (routing, interceptors); render router outlet; AppComponent provides global theme and effects

**Routing Entry Points:**
- `/lobby` (default): `LobbyComponent` - player registration
- `/game`: `GameComponent` - role reveal and discussion phases
- `/voting`: `VotingComponent` - voting phase
- `/result`: `ResultComponent` - game end screen
- `/login`: `LoginComponent` - authentication
- `/categories`: `CategoriesComponent` - category management (auth-guarded)

## Error Handling

**Strategy:** Result-tuple pattern on backend; RxJS catchError on frontend; try-catch in critical async operations.

**Patterns:**

Backend:
- Managers return `(bool Success, string Message)` tuples for non-critical operations (e.g., `RegisterPlayer`, `CastVote`)
- Controllers wrap responses in `ApiResponse<T>` DTO with `success`, `message`, and `data` fields
- Critical failures (e.g., invalid phase transition) trigger HTTP 400 `BadRequest`
- Example: `GameController.RegisterPlayer()` checks manager result, returns `BadRequest` if `Success == false`

Frontend:
- `GameService` methods map API responses to models via `.pipe(map(res => res.data!))`
- Components subscribe with `{ next, error }` handlers; errors set local `error` string for UI display
- `GameStateService.refreshState()` wraps in `catchError()`, logs to console, returns `of(null)` to prevent polling collapse
- Example: `LobbyComponent.addPlayer()` sets `this.error` on HTTP error, displays to user via template

## Cross-Cutting Concerns

**Logging:**
- Backend: Console output in `Program.cs` startup message; `GameSessionManager.CleanupExpiredSessions()` logs cleanup events
- Frontend: `console.error()` in service catchError handlers; no persistent logging

**Validation:**
- Backend: Phase guards (e.g., `RegisterPlayer` checks `CurrentPhase == GamePhase.Lobby`)
- Backend: Player count bounds (min/max via `GameSettings`)
- Backend: Vote validation (no self-votes, no voting for eliminated players, must be active to vote)
- Frontend: Component-level validation (e.g., `LobbyComponent.addPlayer()` checks `trim().length > 0`)
- Frontend: `authGuard` on `/categories` route requires authentication

**Authentication:**
- JWT tokens issued by `AuthService.Login()` / `AuthService.Register()`
- `authInterceptor` adds `Authorization: Bearer {token}` header to requests if token exists in localStorage
- `JwtSettings` configured in `appsettings.json`; validated on every request via `JwtBearerDefaults` middleware
- Protected resources: `CategoriesController`, `/categories` frontend route

**Session Management:**
- Backend: `X-Session-Id` header identifies game session; generated as GUID in `SessionControllerBase.GetOrCreateSession()` if missing
- Frontend: `SessionService` persists session ID in localStorage; `sessionInterceptor` injects on every request
- Cleanup: `GameSessionManager` removes sessions idle for 4+ hours

---

*Architecture analysis: 2026-04-13*
