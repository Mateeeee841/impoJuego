# Codebase Structure

**Analysis Date:** 2026-04-13

## Directory Layout

```
ImpoJuego/
├── impojuego/                          # .NET 8 backend solution
│   ├── impojuego/                      # Core library (game logic)
│   │   ├── Config/                     # Configuration
│   │   │   └── GameSettings.cs
│   │   ├── Data/                       # Persistence layer
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Category.cs
│   │   │   │   └── Word.cs
│   │   │   └── WordCategories.cs       # Static category/word data
│   │   ├── Models/                     # Game domain models
│   │   │   ├── Player.cs
│   │   │   ├── GameSession.cs
│   │   │   ├── GamePhase.cs            # Enum: Lobby, RoleReveal, Discussion, Voting, Finished
│   │   │   └── GameRole.cs             # Enum: Crewmate, Impostor
│   │   ├── Managers/                   # Game orchestration
│   │   │   ├── GameManager.cs          # Main orchestrator
│   │   │   ├── GameSessionManager.cs   # Multi-session coordination
│   │   │   ├── PlayerManager.cs        # Player registration, role assignment
│   │   │   ├── VotingManager.cs        # Vote collection and tally
│   │   │   └── MenuManager.cs          # Game reset/navigation actions
│   │   └── Program.cs                  # Console entry point (for testing)
│   │
│   ├── ImpoJuego.Api/                  # ASP.NET Core API project
│   │   ├── Program.cs                  # Startup, service registration, middleware
│   │   ├── Config/
│   │   │   └── JwtSettings.cs          # JWT authentication config
│   │   ├── Controllers/
│   │   │   ├── SessionControllerBase.cs # Base class with X-Session-Id handling
│   │   │   ├── GameController.cs       # Lobby, game flow, voting endpoints
│   │   │   ├── MenuController.cs       # Reset/navigation endpoints
│   │   │   ├── AuthController.cs       # Login/register
│   │   │   └── CategoriesController.cs # Category CRUD
│   │   ├── DTOs/
│   │   │   └── GameDTOs.cs             # Request/response DTOs (LobbyStatusDto, etc.)
│   │   ├── Data/
│   │   │   ├── ImpoJuegoDbContext.cs   # Entity Framework context (SQLite)
│   │   │   └── DbSeeder.cs             # Initial data seeding
│   │   ├── Services/
│   │   │   ├── AuthService.cs          # JWT token generation, user auth
│   │   │   └── CategoryService.cs      # Category business logic
│   │   └── Properties/
│   │       └── launchSettings.json
│   │
│   ├── ImpoJuego.Tests/                # xUnit test project
│   │   ├── *Tests.cs                   # Test files (PlayerTests, GameManagerTests, etc.)
│   │   └── coverlet.runsettings        # Code coverage config
│   │
│   └── impojuego.sln                   # Solution file
│
├── impojuego-web/                      # Angular 17 frontend
│   ├── src/
│   │   ├── main.ts                     # Bootstrap entry point
│   │   ├── index.html                  # HTML template
│   │   ├── styles.scss                 # Global styles (theme, animations, reusable classes)
│   │   ├── assets/                     # Images, fonts
│   │   │
│   │   └── app/
│   │       ├── app.component.ts        # Root component (theme, effects)
│   │       ├── app.component.html      # Router outlet
│   │       ├── app.config.ts           # Angular config, interceptor providers, CONFIG object
│   │       ├── app.routes.ts           # Route definitions
│   │       │
│   │       ├── components/             # Routable standalone components
│   │       │   ├── lobby/              # Player registration (GET /lobby, POST /players)
│   │       │   │   ├── lobby.component.ts
│   │       │   │   ├── lobby.component.html
│   │       │   │   └── lobby.component.scss
│   │       │   ├── game/               # Role reveal & discussion (RoleReveal phase)
│   │       │   │   ├── game.component.ts
│   │       │   │   ├── game.component.html
│   │       │   │   └── game.component.scss
│   │       │   ├── voting/             # Vote casting (Voting phase)
│   │       │   │   ├── voting.component.ts
│   │       │   │   ├── voting.component.html
│   │       │   │   └── voting.component.scss
│   │       │   ├── result/             # Game end screen (Finished phase)
│   │       │   │   ├── result.component.ts
│   │       │   │   ├── result.component.html
│   │       │   │   └── result.component.scss
│   │       │   ├── login/              # Authentication
│   │       │   │   ├── login.component.ts
│   │       │   │   ├── login.component.html
│   │       │   │   └── login.component.scss
│   │       │   └── categories/         # Category management (protected route)
│   │       │       ├── categories.component.ts
│   │       │       ├── categories.component.html
│   │       │       └── categories.component.scss
│   │       │
│   │       ├── services/               # Singleton services
│   │       │   ├── game.service.ts     # API calls for game endpoints
│   │       │   ├── game-state.service.ts # Polling, state management, navigation
│   │       │   ├── session.service.ts  # Session ID persistence
│   │       │   ├── auth.service.ts     # JWT token management
│   │       │   └── categories.service.ts # Category API calls
│   │       │
│   │       ├── interceptors/           # HTTP interceptors
│   │       │   ├── session.interceptor.ts # Injects X-Session-Id header
│   │       │   └── auth.interceptor.ts   # Injects Authorization header
│   │       │
│   │       ├── guards/                 # Route guards
│   │       │   └── auth.guard.ts       # Requires authentication for protected routes
│   │       │
│   │       └── models/
│   │           ├── index.ts
│   │           └── game.models.ts      # TypeScript interfaces (ApiResponse, GameState, etc.)
│   │
│   ├── angular.json                    # Angular CLI configuration
│   ├── tsconfig.json                   # TypeScript configuration
│   ├── package.json
│   └── dist/                           # Production build output

└── .planning/
    └── codebase/
        ├── ARCHITECTURE.md
        └── STRUCTURE.md
```

## Directory Purposes

**Backend Core (`impojuego/impojuego/`):**
- Purpose: Pure game logic, zero HTTP/UI dependencies
- Contains: Game state machines, managers, models
- Key files: `GameManager.cs` (orchestrator), `GameSessionManager.cs` (multi-session), `Player.cs`, `GamePhase.cs`

**Backend API (`impojuego/ImpoJuego.Api/`):**
- Purpose: REST API layer exposing game logic
- Contains: Controllers, DTOs, services (auth/categories), database
- Key files: `GameController.cs` (endpoints), `SessionControllerBase.cs` (session handling), `Program.cs` (startup)

**Backend Tests (`impojuego/ImpoJuego.Tests/`):**
- Purpose: Unit test coverage for game logic
- Contains: xUnit tests with 100% line coverage
- Key files: Test files organized by component (PlayerTests, GameManagerTests, etc.)

**Frontend Components (`impojuego-web/src/app/components/`):**
- Purpose: UI rendered per game phase
- Key files: One folder per routable phase
  - `lobby/`: `/lobby` route - player registration
  - `game/`: `/game` route - role reveal & discussion
  - `voting/`: `/voting` route - vote casting
  - `result/`: `/result` route - game end
  - `login/`: `/login` route - authentication
  - `categories/`: `/categories` route - category management (auth-guarded)

**Frontend Services (`impojuego-web/src/app/services/`):**
- Purpose: Business logic, API communication, state management
- Key files:
  - `game.service.ts`: HTTP client for all game endpoints
  - `game-state.service.ts`: Polling loop, phase-based navigation, localStorage persistence
  - `session.service.ts`: Session ID persistence
  - `auth.service.ts`: JWT token management
  - `categories.service.ts`: Category CRUD

**Frontend Interceptors (`impojuego-web/src/app/interceptors/`):**
- Purpose: Transparent header injection for every HTTP request
- Key files:
  - `session.interceptor.ts`: Adds `X-Session-Id` header from SessionService
  - `auth.interceptor.ts`: Adds `Authorization: Bearer` header if token exists

**Frontend Guards (`impojuego-web/src/app/guards/`):**
- Purpose: Route protection
- Key files:
  - `auth.guard.ts`: Blocks access to `/categories` if not authenticated

**Frontend Models (`impojuego-web/src/app/models/`):**
- Purpose: TypeScript interfaces mirroring backend DTOs
- Key files: `game.models.ts` (ApiResponse, GameState, Player, etc.)

## Key File Locations

**Entry Points:**

Backend API:
- `impojuego/ImpoJuego.Api/Program.cs`: Startup, service registration, middleware configuration

Backend Console (testing):
- `impojuego/impojuego/Program.cs`: Manual gameplay loop

Frontend:
- `impojuego-web/src/main.ts`: Bootstrap entry point
- `impojuego-web/src/app/app.component.ts`: Root component with global theme/effects

**Configuration:**

Backend:
- `impojuego/ImpoJuego.Api/Program.cs`: GameSettings instance, JWT config, CORS, database
- `impojuego/impojuego/Config/GameSettings.cs`: Game parameters (min/max players, impostor probability)

Frontend:
- `impojuego-web/src/app/app.config.ts`: Angular providers (routing, interceptors), CONFIG object with API URL
- `impojuego-web/src/app/app.routes.ts`: Route definitions

**Core Logic:**

Backend game engine:
- `impojuego/impojuego/Managers/GameManager.cs`: Main orchestrator
- `impojuego/impojuego/Managers/GameSessionManager.cs`: Multi-session management
- `impojuego/impojuego/Managers/PlayerManager.cs`: Player registration, role assignment
- `impojuego/impojuego/Managers/VotingManager.cs`: Vote logic

Backend API layer:
- `impojuego/ImpoJuego.Api/Controllers/GameController.cs`: Lobby, game flow, voting endpoints
- `impojuego/ImpoJuego.Api/Controllers/SessionControllerBase.cs`: Shared session handling

Frontend state:
- `impojuego-web/src/app/services/game-state.service.ts`: Single source of truth (BehaviorSubject + polling)
- `impojuego-web/src/app/services/game.service.ts`: HTTP client wrapper

**Testing:**

Backend:
- `impojuego/ImpoJuego.Tests/`: All test files; run with `dotnet test`
- `impojuego/ImpoJuego.Tests/coverlet.runsettings`: Coverage configuration (100% line coverage target)

Frontend:
- Component `.spec.ts` files (not shown in directory listing but would follow `[name].component.spec.ts` pattern)

**Styling & Theme:**

Frontend:
- `impojuego-web/src/styles.scss`: Global styles (dark cinematic theme, reusable `.impostor-btn`, `.impostor-card`, etc.)
- `impojuego-web/src/app/app.component.scss`: Root-level styles
- Component-level: Each component has `.component.scss` for local styles

## Naming Conventions

**Files:**

Backend:
- `.cs` files: PascalCase (e.g., `GameManager.cs`, `PlayerTests.cs`)
- Namespaces: `ImpoJuego.Managers`, `ImpoJuego.Models`, `ImpoJuego.Api.Controllers`

Frontend:
- TypeScript: camelCase (e.g., `game.service.ts`, `session.interceptor.ts`)
- Components: `[name].component.ts`, `[name].component.html`, `[name].component.scss`
- Services: `[name].service.ts`
- Interceptors: `[name].interceptor.ts`
- Guards: `[name].guard.ts`
- Models: `[name].models.ts`

**Directories:**

Backend:
- Feature directories: `Config`, `Data`, `Models`, `Managers`
- Logical grouping in API: `Controllers`, `DTOs`, `Services`, `Data`

Frontend:
- Feature directories: `components`, `services`, `interceptors`, `guards`, `models`
- Feature sub-directories: One per route (`lobby`, `game`, `voting`, `result`, `login`, `categories`)

## Where to Add New Code

**New Game Feature (e.g., new phase or manager responsibility):**

1. Core logic layer:
   - Add model or state enum in `impojuego/impojuego/Models/` (e.g., `NewFeature.cs`)
   - Add manager or enhance existing manager in `impojuego/impojuego/Managers/` (e.g., `GameManager.cs`)
   - Add tests in `impojuego/ImpoJuego.Tests/` (e.g., `GameManagerTests.cs` new test case)

2. API layer:
   - Add endpoint in appropriate controller in `impojuego/ImpoJuego.Api/Controllers/` (e.g., `GameController.cs`)
   - Add DTO in `impojuego/ImpoJuego.Api/DTOs/GameDTOs.cs` if needed

3. Frontend layer:
   - Add TypeScript model in `impojuego-web/src/app/models/game.models.ts`
   - Add service method in `impojuego-web/src/app/services/game.service.ts`
   - If new phase: Create component in `impojuego-web/src/app/components/[phase-name]/`
   - If new phase: Add route in `impojuego-web/src/app/app.routes.ts`
   - If new phase: Add routing logic in `impojuego-web/src/app/services/game-state.service.ts` `navigateToCurrentPhase()`

**New Component:**

Frontend standalone component:
- Create folder: `impojuego-web/src/app/components/[feature-name]/`
- Create files: `[feature-name].component.ts`, `[feature-name].component.html`, `[feature-name].component.scss`
- Use Angular 17 standalone API: `@Component({ standalone: true, imports: [...] })`
- Add route in `app.routes.ts` if routable
- Inject services via constructor

**Utilities & Helpers:**

Shared utilities:
- Backend: Add to existing managers or create new manager class in `impojuego/impojuego/Managers/`
- Frontend: Create new service in `impojuego-web/src/app/services/` if stateful; use utility functions in services if stateless

## Special Directories

**`/impojuego/obj/` and `/impojuego/bin/`:**
- Purpose: Build artifacts
- Generated: Yes
- Committed: No (in .gitignore)

**`/impojuego-web/node_modules/`:**
- Purpose: npm dependencies
- Generated: Yes
- Committed: No (in .gitignore)

**`/impojuego-web/dist/`:**
- Purpose: Production build output
- Generated: Yes (`npm run build`)
- Committed: No (in .gitignore)

**`/.planning/codebase/`:**
- Purpose: GSD codebase documentation
- Generated: No (hand-written)
- Committed: Yes

**`/impojuego-web/.angular/cache/`:**
- Purpose: Angular CLI build cache
- Generated: Yes
- Committed: No (in .gitignore)

## Backend-Frontend Mapping

| Backend (Core) | Backend (API) | Frontend |
|---|---|---|
| `GamePhase` enum | Response in `GameState` DTO | `GamePhase` type in `game.models.ts` |
| `Player` model | Serialized in `PlayerDto` | `Player` interface in `game.models.ts` |
| `GameManager.CurrentPhase` | `/api/game/state` returns phase | `GameStateService` polls and routes |
| `GameManager.RegisterPlayer()` | `POST /api/game/players` | `GameService.registerPlayer()`, `LobbyComponent.addPlayer()` |
| `GameManager.StartGame()` | `POST /api/game/start` | `GameService.startGame()`, `LobbyComponent.startGame()` |
| `GameManager.GetPlayerInfo()` | `POST /api/game/reveal` | `GameService.revealRole()`, `GameComponent.revealRole()` |
| `VotingManager.CastVote()` | `POST /api/game/vote` | `GameService.castVote()`, `VotingComponent.castVote()` |
| `GameManager.ProcessVotingResult()` | `POST /api/game/tally` | `GameService.tallyVotes()`, phase auto-advances |
| `GameSessionManager` | Session ID in `X-Session-Id` header | `SessionService` stores in localStorage, `sessionInterceptor` injects |
| `MenuManager.ResetGame()` | `POST /api/menu/reset` | `GameService.resetGame()`, `GameStateService.resetGame()` |

---

*Structure analysis: 2026-04-13*
