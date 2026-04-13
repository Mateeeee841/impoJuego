# Testing Patterns

**Analysis Date:** 2026-04-13

## Test Framework

**Backend Runner:**
- xUnit 2.x (via NuGet)
- Config: `ImpoJuego.Tests/ImpoJuego.Tests.csproj`
- Test assembly: `ImpoJuego.Tests.dll` (.NET 9.0)

**Backend Assertion Library:**
- FluentAssertions (via NuGet)
- Primary assertion method: `.Should()` extension method pattern

**Frontend:**
- Test framework configured but no test files present
- Framework: Karma (via `angular.json` builder)
- Assertion library: Jasmine (~5.1.0)
- Tests configured in `angular.json`: `"tsConfig": "tsconfig.spec.json"`
- **Status:** All test scaffolding skipped via `"skipTests": true` in Angular schematics

**Run Commands:**

Backend (from `impojuego/` directory):
```bash
dotnet test                     # Run all 145 tests
dotnet test --settings ImpoJuego.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"  # With coverage
```

Frontend (from `impojuego-web/` directory):
```bash
npm test                        # Run with Karma (watch mode)
npm test -- --watch=false --browsers=ChromeHeadless  # Single run (no test files present)
```

## Test Execution Results

**Backend Test Run (2026-04-13):**
```
Serie de pruebas para C:\_Mateo\_Mateo\ImpoJuego\impojuego\ImpoJuego.Tests\bin\Debug\net9.0\ImpoJuego.Tests.dll (.NETCoreApp,Version=v9.0)
Versión 17.13.0 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:   145, Omitido:     0, Total:   145, Duración: 401 ms - ImpoJuego.Tests.dll (net9.0)
```

**Status:** All 145 tests passing in 401ms. No failures, no skips.

**Frontend Test Run:**
```
npm test -- --watch=false --browsers=ChromeHeadless

An unhandled exception occurred: error TS18003: No inputs were found in config file 'C:/_Mateo/_Mateo/ImpoJuego/impojuego-web/tsconfig.spec.json'. Specified 'include' paths were '["src/**/*.spec.ts","src/**/*.d.ts"]'
```

**Status:** No frontend test files exist. Karma configured but no `.spec.ts` files found in `src/`.

## Test File Organization

**Backend Location:**
- All tests in: `ImpoJuego.Tests/` project
- Test files: `ImpoJuego.Tests/*.cs`

**Naming Convention:**
- `*Tests.cs` suffix: `PlayerTests.cs`, `PlayerManagerTests.cs`, `GameManagerTests.cs`
- Namespace: `namespace ImpoJuego.Tests;` (file-scoped)

**Structure:**
```
ImpoJuego.Tests/
├── PlayerTests.cs                    (8 tests)
├── PlayerManagerTests.cs             (24 tests)
├── GameManagerTests.cs               (32 tests)
├── VotingManagerTests.cs             (19 tests)
├── GameSessionTests.cs               (15 tests)
├── GameSettingsTests.cs              (5 tests)
├── WordCategoriesTests.cs            (9 tests)
├── EntitiesTests.cs                  (8 tests)
├── MenuManagerTests.cs               (11 tests)
├── ImpoJuego.Tests.csproj
└── coverlet.runsettings              (coverage config)
```

**Frontend Structure:**
```
impojuego-web/
├── src/
│   ├── app/
│   │   ├── components/         (no .spec.ts files)
│   │   ├── services/           (no .spec.ts files)
│   │   ├── models/
│   │   ├── interceptors/
│   │   └── guards/
│   └── ...
├── angular.json                (Karma test configuration)
└── tsconfig.spec.json          (spec TypeScript config - references no files)
```

## Test Structure

**xUnit Test Class Pattern:**

```csharp
public class PlayerManagerTests
{
    private PlayerManager _manager;

    public PlayerManagerTests()
    {
        _manager = new PlayerManager();
    }

    [Fact]
    public void RegisterPlayer_WithValidName_ShouldSucceed()
    {
        var (success, message) = _manager.RegisterPlayer("Player1");

        success.Should().BeTrue();
        message.Should().Contain("registrado");
        _manager.Count.Should().Be(1);
    }

    [Fact]
    public void RegisterPlayer_WithEmptyName_ShouldFail()
    {
        var (success, message) = _manager.RegisterPlayer("");

        success.Should().BeFalse();
        message.Should().Contain("vacío");
    }
}
```

**Key Patterns:**
- Private field initialization: `_manager = new PlayerManager();`
- Parameterless constructor for test setup (xUnit style)
- One assertion focus per test (`RegisterPlayer_WithEmptyName_ShouldFail`)
- Naming: `MethodName_Condition_ExpectedBehavior`

**Fixture Setup Pattern:**

```csharp
public class GameManagerTests
{
    private GameManager _game;

    public GameManagerTests()
    {
        _game = new GameManager();
    }

    private void SetupMinimumPlayers()
    {
        _game.RegisterPlayer("Player1");
        _game.RegisterPlayer("Player2");
        _game.RegisterPlayer("Player3");
    }

    private Dictionary<string, List<string>> GetTestCategories()
    {
        return new Dictionary<string, List<string>>
        {
            { "Animales", new List<string> { "Perro", "Gato", "Pájaro" } },
            { "Frutas", new List<string> { "Manzana", "Banana", "Naranja" } }
        };
    }

    [Fact]
    public void StartGame_WithMinimumPlayers_ShouldSucceed()
    {
        SetupMinimumPlayers();

        var (success, message) = _game.StartGame(GetTestCategories());

        success.Should().BeTrue();
        _game.CurrentPhase.Should().Be(GamePhase.RoleReveal);
    }
}
```

**Teardown Pattern:**

```csharp
public class GameSessionManagerTests : IDisposable
{
    private GameSessionManager _manager;

    public GameSessionManagerTests()
    {
        _manager = new GameSessionManager(new GameSettings());
    }

    public void Dispose()
    {
        _manager.Dispose();
    }

    [Fact]
    public void GetOrCreateSession_ShouldCreateNewSession()
    {
        var session = _manager.GetOrCreateSession("new-session");
        session.Should().NotBeNull();
    }
}
```

Implements `IDisposable` for cleanup.

**Assertion Pattern:**

```csharp
// Single value assertions
success.Should().BeTrue();
_manager.Count.Should().Be(1);
_game.CurrentPhase.Should().Be(GamePhase.RoleReveal);

// String assertions
message.Should().Contain("registrado");
message.Should().NotBeNullOrEmpty();

// Collection assertions
_manager.Players.Should().HaveCount(3);
players.Should().NotBeEmpty();

// Object assertions
player.Should().NotBeNull();
session.Should().BeSameAs(session2);

// Numeric assertions
session.LastAccessedAt.Should().BeAfter(originalTime);
session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

// Enum assertions
player.Role.Should().Be(GameRole.Impostor);
```

## Mocking

**Framework:** No external mocking framework detected
- Developers create test doubles manually or use Arrange-Act-Assert without mocks

**Test Data Pattern:**

```csharp
private void SetupMinimumPlayers()
{
    _game.RegisterPlayer("Player1");
    _game.RegisterPlayer("Player2");
    _game.RegisterPlayer("Player3");
}

private Dictionary<string, List<string>> GetTestCategories()
{
    return new Dictionary<string, List<string>>
    {
        { "Animales", new List<string> { "Perro", "Gato", "Pájaro" } },
        { "Frutas", new List<string> { "Manzana", "Banana", "Naranja" } }
    };
}
```

**What to Mock:**
- No mocking observed; tests use real objects
- Database calls tested via EF Core test context (if present)

**What NOT to Mock:**
- Game logic managers tested with real implementations
- Player objects created and used directly
- No test doubles for strategy verification

## Fixtures and Factories

**Test Data Approach:**
- Manual object construction in setup methods
- Consistent fixture names: `_game`, `_manager`, `_player1`, `_playerManager`
- Data defined in private `GetTestCategories()` methods

**Location:**
- Fixtures defined at test class level as private fields
- Setup in parameterless constructor or private helper methods
- No separate factory classes detected

## Coverage

**Requirements:**
- Target: 100% line coverage (stated in CLAUDE.md)
- Branch coverage: 99.19% (from latest run)
- Reported via Coverlet code coverage

**View Coverage:**

Backend:
```bash
cd impojuego
dotnet test --settings ImpoJuego.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"
```

Output directory: `TestResults/` contains `coverage.cobertura.xml`

**Configuration File:**
`ImpoJuego.Tests/coverlet.runsettings`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[ImpoJuego.Api]*,[*]*.Program</Exclude>
          <ExcludeByFile>**/Program.cs</ExcludeByFile>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

**Exclusions:**
- `ImpoJuego.Api` project (API layer, not core logic)
- `Program.cs` files (infrastructure setup)

Frontend:
```bash
cd impojuego-web
npm test -- --code-coverage --watch=false --browsers=ChromeHeadless
```

## Test Types

**Unit Tests (Primary):**
- Scope: Individual manager/service classes
- Approach: Arrange-Act-Assert with real dependencies
- Examples:
  - `PlayerManagerTests` - Player registration, retrieval, removal
  - `GameSettingsTests` - Configuration validation
  - `VotingManagerTests` - Vote casting and tally logic

**Integration Tests (Limited):**
- `GameSessionManagerTests` - Session lifecycle with cleanup
- `GameManagerTests` - Full game flow (lobby → voting)
- Tests verify state transitions and side effects

**E2E Tests (Not Present):**
- No end-to-end tests found
- No Playwright, Cypress, or Angular E2E configured
- Manual API testing via Swagger UI documented

## Common Patterns

**Async Testing (Limited):**
- Most tests are synchronous
- No `[Fact]` async patterns detected
- Managers use synchronous game logic

**Error Testing Pattern:**

```csharp
[Fact]
public void RegisterPlayer_WithEmptyName_ShouldFail()
{
    var (success, message) = _manager.RegisterPlayer("");

    success.Should().BeFalse();
    message.Should().Contain("vacío");
}

[Fact]
public void CastVote_ByEliminatedPlayer_ShouldFail()
{
    _player1.Eliminate();

    var (success, message) = _votingManager.CastVote(_player1, _player2);

    success.Should().BeFalse();
    message.Should().Contain("eliminados no pueden votar");
}
```

Standard approach:
1. Setup invalid state or input
2. Call method
3. Assert `success.Should().BeFalse()`
4. Assert error message contains expected Spanish text

**State Transition Testing:**

```csharp
[Fact]
public void StartGame_WithMinimumPlayers_ShouldSucceed()
{
    SetupMinimumPlayers();

    var (success, message) = _game.StartGame(GetTestCategories());

    success.Should().BeTrue();
    _game.CurrentPhase.Should().Be(GamePhase.RoleReveal);  // Phase changed
    _game.RoundNumber.Should().Be(1);                       // Round incremented
    _game.CurrentCategory.Should().NotBeNullOrEmpty();      // Category set
    _game.CurrentWord.Should().NotBeNullOrEmpty();          // Word selected
}
```

## Test Coverage Breakdown

**By Component (145 tests total):**

- **PlayerTests** (8 tests): `src/impojuego/Models/Player.cs`
  - Role assignment, elimination, round tracking

- **PlayerManagerTests** (24 tests): `src/impojuego/Managers/PlayerManager.cs`
  - Registration, removal, role assignment, queries

- **GameManagerTests** (32 tests): `src/impojuego/Managers/GameManager.cs`
  - Game start, phase transitions, role reveal, voting flow

- **VotingManagerTests** (19 tests): `src/impojuego/Managers/VotingManager.cs`
  - Vote casting, vote tally, skip logic, tie handling

- **GameSessionTests** (15 tests): `src/impojuego/Models/GameSession.cs` + `src/impojuego/Managers/GameSessionManager.cs`
  - Session creation, expiration, cleanup

- **GameSettingsTests** (5 tests): `src/impojuego/Config/GameSettings.cs`
  - Configuration validation, defaults

- **WordCategoriesTests** (9 tests): `src/impojuego/Data/WordCategories.cs`
  - Category and word retrieval

- **MenuManagerTests** (11 tests): `src/impojuego/Managers/MenuManager.cs`
  - Game reset, menu state transitions

- **EntitiesTests** (8 tests): `src/impojuego/Data/Entities/*.cs`
  - User, Category, Word entity validation

---

*Testing analysis: 2026-04-13*
