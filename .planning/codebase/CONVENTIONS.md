# Coding Conventions

**Analysis Date:** 2026-04-13

## Naming Patterns

**Files (C#):**
- PascalCase: `Player.cs`, `GameManager.cs`, `PlayerManagerTests.cs`
- Organized by feature/function: `Managers/`, `Models/`, `Services/`, `Controllers/`, `Data/`

**Files (TypeScript/Angular):**
- kebab-case with feature suffix: `game.service.ts`, `game-state.service.ts`, `session.interceptor.ts`
- Components: `app.component.ts`, `game.component.ts`, `lobby.component.ts`

**Functions (C#):**
- PascalCase for public methods: `RegisterPlayer()`, `StartGame()`, `CastVote()`
- PascalCase for private methods: `GetTestCategories()`, `SetupMinimumPlayers()`
- Tuple-returning methods return `(bool Success, string Message)` pattern for operation results

**Functions (TypeScript):**
- camelCase for all methods: `registerPlayer()`, `startGame()`, `castVote()`
- Private methods prefixed with underscore in some cases (conventions vary in service)
- Async operations return `Observable<T>` from RxJS

**Variables (C#):**
- camelCase for locals: `(success, message)`, `normalizedName`, `player1`
- PascalCase for properties: `Name`, `IsEliminated`, `Players`, `CurrentPhase`
- Readonly fields prefixed with underscore: `_players`, `_random`, `_playerLookup`

**Variables (TypeScript):**
- camelCase for all: `gameState`, `currentPlayerName`, `sessionId`
- Private fields prefixed with underscore: `_players`, `_gameState$`, `_pollSubscription`
- Observable subjects use `$` suffix: `gameState$`, `isPolling`

**Types (C#):**
- PascalCase enums: `GamePhase`, `GameRole`, `GameResult`
- Enum values in PascalCase: `Lobby`, `RoleReveal`, `Impostor`, `Crewmate`
- Record types in PascalCase with Dto suffix: `ApiResponse<T>`, `LobbyStatusDto`, `PlayerRoleDto`

**Types (TypeScript):**
- PascalCase interfaces: `ApiResponse<T>`, `LobbyStatus`, `PlayerRole`, `GameState`
- Union types for phase: `GamePhase = 'Lobby' | 'RoleReveal' | 'Discussion' | 'Voting' | 'Finished'`
- Nullable properties use `| null`: `secretWord: string | null`, `fellowImpostors: string[] | null`

## Code Style

**Formatting (C#):**
- No explicit formatting configuration detected
- Consistent: 4-space indentation, opening braces on same line (Allman style avoided)
- File-scoped namespaces: `namespace ImpoJuego.Models;`
- No trailing semicolons on namespace declarations

**Formatting (TypeScript):**
- No .prettierrc or .eslintrc found; Angular defaults likely in use
- Consistent: 2-space indentation in templates/JSON, 4-space in code
- Trailing semicolons present

**Linting (C#):**
- No explicit analyzer configuration found
- Code follows standard C# conventions
- XML documentation comments used consistently

**Linting (TypeScript):**
- TypeScript strict mode enabled: `"strict": true` in `tsconfig.json`
- Additional strict options: `noImplicitOverride`, `noPropertyAccessFromIndexSignature`, `noImplicitReturns`, `noFallthroughCasesInSwitch`
- Angular strict template checking: `strictTemplates: true`

## Import Organization

**C# Order:**
1. System/Microsoft namespaces
2. Third-party namespaces (ImpoJuego, FluentAssertions, Entity Framework)
3. Internal ImpoJuego namespaces

Example from `GameController.cs`:
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Api.DTOs;
using ImpoJuego.Api.Services;
using ImpoJuego.Managers;
using ImpoJuego.Models;
using ImpoJuego.Data;
```

**TypeScript Order:**
1. Angular core imports
2. RxJS imports
3. Internal service/model imports
4. Local config imports

Example from `game.service.ts`:
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ApiResponse,
  LobbyStatus,
  GameStarted,
  // ... more model imports
} from '../models';
import { CONFIG } from '../../app/app.config'
```

**Path Aliases:**
- Not explicitly configured in `tsconfig.json` (no `paths` mapping found)
- Relative paths used throughout: `../models`, `../services`, `../../app/`

## Error Handling

**C# Pattern - Operation Result Tuples:**
Functions return `(bool Success, string Message)` tuples for operations that can fail:
```csharp
public (bool Success, string Message) RegisterPlayer(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return (false, "El nombre no puede estar vacío");
    
    // ... success case
    return (true, $"Jugador '{player.Name}' registrado");
}
```

Used in: `RegisterPlayer()`, `RemovePlayer()`, `CastVote()`, `StartGame()`

**API Response Wrapper:**
All API responses wrapped in `ApiResponse<T>` record:
```csharp
public record ApiResponse<T>(bool Success, string Message, T? Data = default);
```

Controllers return structured responses:
```csharp
return Ok(new ApiResponse<LobbyStatusDto>(true, "Lobby status", data));
return BadRequest(new ApiResponse<LobbyStatusDto>(false, message, null));
```

**TypeScript Pattern - RxJS Error Handling:**
Services use RxJS `catchError` operator:
```typescript
refreshState(): Observable<GameState | null> {
    return this.gameService.getGameState().pipe(
        tap(state => { /* ... */ }),
        catchError(err => {
            console.error('Error refreshing game state:', err);
            return of(null);
        })
    );
}
```

Component error messages stored in error property:
```typescript
error = '';
// In subscription:
error: () => this.error = 'Error al cargar estado del juego'
```

## Logging

**Framework:** `console` (browser) for frontend, no explicit logging framework detected for backend

**C# Patterns:**
- No logging detected in game logic or managers
- Validation errors returned via operation result tuples
- All error messages in Spanish

**TypeScript Patterns:**
- `console.error()` for error logging in services and components
- Example: `console.error('Error refreshing game state:', err);`
- `console.error('Error syncing state:', err);`
- Error context strings in Spanish for user-facing UI

## Comments

**When to Comment:**
- Used for class-level documentation
- Used for public method documentation
- Spanish language throughout

**Documentation Style (C#):**
Three-slash XML documentation comments above public types and methods:
```csharp
/// <summary>
/// Representa un jugador en el juego
/// </summary>
public class Player
{
    // ...
}

/// <summary>
/// Registra un nuevo jugador
/// </summary>
public (bool Success, string Message) RegisterPlayer(string name)
```

**Inline Comments (C#):**
Used for explaining sections in complex managers:
```csharp
// === LOBBY ENDPOINTS ===
// === GAME FLOW ===
// === VOTING ===
// === GAME END ===
```

Section markers use three equals: `// === SECTION NAME ===`

**JSDoc/TSDoc (TypeScript):**
Used in services and state management:
```typescript
/**
 * Carga el estado del servidor y actualiza el BehaviorSubject
 */
refreshState(): Observable<GameState | null> {
    // ...
}
```

**Inline Comments (TypeScript):**
Sparse; focus on variable initialization in tests:
```typescript
// Suscribirse a cambios de estado
this.stateSubscription = this.gameStateService.getState().subscribe(state => {
```

## Function Design

**Size (C#):**
- Methods average 10-30 lines
- Complex logic in manager classes (`GameManager`) reasonably long but well-structured
- Private helper methods extracted for readability

**Parameters (C#):**
- Minimal parameters; use object properties for state access
- Operation methods use nullable reference types: `Task<(User? user, string? error)>`
- DTOs used for API contracts

**Return Values (C#):**
- Tuple returns for operations: `(bool, string)`
- Objects for data retrieval
- Async methods return `Task<T>` or `Task`

**Parameters (TypeScript):**
- Services use dependency injection via constructor
- Methods use single parameters or destructured objects
- No parameters passed for simple operations

**Return Values (TypeScript):**
- Observable<T> for all async operations
- Void for state updates (side effects via rxjs tap operator)
- Null returns in catchError patterns

## Module Design

**Exports (C#):**
- Public classes explicitly declared
- Use of internal namespace for feature isolation
- Interfaces for service contracts: `IAuthService`, `ICategoryService`

**Exports (TypeScript):**
- Single export per file (services, components, interceptors)
- Re-exported models through `index.ts` barrel file:
  - `src/app/models/index.ts` exports all interfaces

**Barrel Files:**
- `src/app/models/index.ts` used for centralizing model imports:
  ```typescript
  export { ApiResponse, LobbyStatus, GameStarted, PlayerRole, GameState, ... }
  ```
- Simplifies imports in services: `import { ApiResponse, ... } from '../models'`

## Language & Localization

**Spanish Language Throughout:**
- **C# Code:**
  - XML doc comments: `/// <summary>Representa un jugador en el juego</summary>`
  - Error messages: `"El nombre no puede estar vacío"`, `"Ya existe un jugador"`
  - Variable/property names in Spanish where appropriate (enum comments)
  - Configuration comments: `// Quedan solo impostores o empate en números`

- **TypeScript/Angular Code:**
  - Inline comments in Spanish: `// Suscribirse a cambios de estado`
  - JSDoc comments in Spanish: `/** Carga el estado del servidor... */`
  - Console logs in English (error handling)

- **UI/Messages:**
  - All user-facing messages in Spanish
  - Phase names in Spanish in code comments but English in type definitions
  - Error messages: `"Error al cargar estado del juego"`, `"Error al obtener estado del juego"`

## Dependency Injection

**C# Pattern:**
Constructor injection used throughout:
```csharp
public GameController(GameSessionManager sessionManager, ICategoryService categoryService) 
    : base(sessionManager)
{
    _categoryService = categoryService;
}

public AuthService(ImpoJuegoDbContext context, IOptions<JwtSettings> jwtSettings)
{
    _context = context;
    _jwtSettings = jwtSettings.Value;
}
```

Registered in `Program.cs`:
```csharp
builder.Services.AddSingleton<GameSessionManager>(sp =>
    new GameSessionManager(gameSettings, TimeSpan.FromHours(4)));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
```

**TypeScript Pattern:**
Angular's providedIn dependency injection:
```typescript
@Injectable({
  providedIn: 'root'
})
export class GameService {
    constructor(private http: HttpClient) {}
}
```

Configured in `app.config.ts`:
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([sessionInterceptor, authInterceptor]))
  ]
};
```

---

*Convention analysis: 2026-04-13*
