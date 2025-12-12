# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ImpoJuego is an "Impostor" party game (similar to Among Us or Spyfall) with a .NET 8 backend API and Angular 17 frontend. Players are assigned roles (Crewmate or Impostor), with Crewmates knowing a secret word from a category while Impostors must bluff without knowing the word.

## Development Commands

### Backend (.NET API)
```bash
# Run the API (from impojuego/ImpoJuego.Api/)
dotnet run

# Build solution (from impojuego/)
dotnet build impojuego.sln

# Run with watch for development
dotnet watch run --project ImpoJuego.Api
```
API runs on http://localhost:5000 with Swagger UI at root.

### Frontend (Angular)
```bash
# From impojuego-web/
npm install
npm start          # ng serve - runs on http://localhost:4200
npm run build      # production build to dist/
npm test           # unit tests via Karma
```

## Architecture

### Backend Structure (`impojuego/`)
Two projects in solution:
- **impojuego** (Core library): Game logic, models, managers
- **ImpoJuego.Api**: ASP.NET Core Web API exposing game functionality

Key components:
- `GameManager` - Main orchestrator handling game state, phases, and flow
- `PlayerManager` - Player registration and role assignment
- `VotingManager` - Vote collection and tally logic
- `WordCategories` - Static word/category data (Spanish content)
- `GameSettings` - Configurable game parameters (min/max players, impostor probability)
- `MenuManager` - Game menu actions (reset, full reset, back to lobby)

Game phases: `Lobby` -> `RoleReveal` -> `Discussion` -> `Voting` -> `Finished`

### Frontend Structure (`impojuego-web/`)
Angular 17 standalone components with routing:
- `/lobby` - Player registration
- `/game` - Role reveal and discussion phase
- `/voting` - Vote casting
- `/result` - Game end screen

`GameService` handles all API communication. Production points to Render (`https://impojuego-1.onrender.com/api/game`); for local development, update `game.service.ts` to use `http://localhost:5000/api/game`.

### Frontend Theme ("Among Us Horror")
Dark cinematic aesthetic with space/impostor theme:
- **Palette**: Black (#0a0a0a), blood red (#8B0000), neon purple (#4B0082/#8A2BE2), cold cyan (#00FFFF)
- **Fonts**: Orbitron (titles), Oxanium (UI), Rajdhani (body) - Google Fonts
- **Global effects** in `AppComponent`: Custom red cursor, floating particles, screen shake (15-25s), star parallax
- **Animations**: Page scan reveal, card glitch entrance, neon glow, hover hologram effects
- **Reusable classes** in `styles.scss`: `.impostor-btn`, `.impostor-card`, `.neon-title`, `.glitch-text`

### API Endpoints
Game endpoints under `/api/game`:
- Lobby: `GET /lobby`, `POST /players`, `DELETE /players/{name}`
- Flow: `POST /start`, `GET /state`, `POST /reveal`, `POST /discussion`
- Voting: `POST /voting`, `POST /vote`, `GET /votes`, `POST /tally`
- End: `GET /result`, `POST /reset`, `POST /full-reset`
- Data: `GET /categories`

Menu endpoints under `/api/menu`:
- `GET /options` - Available menu options based on game state
- `POST /reset` - Reset game (keep players)
- `POST /full-reset` - Full reset (clear players)
- `POST /back-to-lobby` - Return to lobby
- `POST /action` - Execute action by name (body: `{"action": "ResetGame"}`)

API responses use `ApiResponse<T>` wrapper with `success`, `message`, and `data` fields.

## Key Design Decisions

- `GameManager` is registered as Singleton - single shared game instance for all clients
- CORS configured for Angular dev server (localhost:4200)
- Spanish language throughout (comments, categories, UI messages)
- Impostors can optionally see fellow impostors (configurable)
- 2-impostor probability kicks in at 5+ players (default 3% chance)

## Deployment

- **Backend**: Docker multi-stage build, deployed to Render at `impojuego-1.onrender.com:5000`
- **Frontend**: Static build (`npm run build`), deployed to Render at `impojuego-web.onrender.com`
- CORS allows: `localhost:4200`, `localhost:5173`, `127.0.0.1:4200`, and Render production domain
