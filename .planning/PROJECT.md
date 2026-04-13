# ImpoJuego

## What This Is

ImpoJuego es un juego de fiesta tipo "Impostor" (estilo Among Us / Spyfall) con backend .NET 8 y frontend Angular 17. Jugadores reciben roles (Crewmate / Impostor); los Crewmates conocen una palabra secreta de una categoría mientras los Impostors tienen que farolear sin conocerla. Deployado en Render (backend + frontend estático).

## Core Value

El juego debe ser jugable end-to-end desde cualquier dispositivo con link compartible, sin requerir setup local. Si esto falla, no sirve para nada.

## Requirements

### Validated

<!-- Funcionalidad ya implementada y con 145 tests backend al 100% de line coverage -->

- ✓ Registro/login con email+password (JWT) — existing
- ✓ CRUD de categorías con palabras (por usuario, privadas) — existing
- ✓ Flujo de juego completo: Lobby → RoleReveal → Discussion → Voting → Finished — existing
- ✓ Multi-sesión via `X-Session-Id` header con cleanup automático — existing
- ✓ Asignación probabilística de 1 o 2 impostores (5+ jugadores) — existing
- ✓ Menú con reset / full-reset / back-to-lobby — existing
- ✓ UI Angular con tema "Among Us Horror" (Orbitron/Oxanium/Rajdhani, paleta negra/roja/cyan) — existing

### Active

<!-- Issues surfaced por /gsd:map-codebase — ver .planning/codebase/CONCERNS.md -->

**Deploy blockers (críticos):**
- [ ] Backend lee `PORT` env var (Render asigna puerto dinámico)
- [ ] SQLite persiste en filesystem que sobrevive redeploys (o migración a Postgres)
- [ ] `render.yaml` en root que define backend + frontend
- [ ] Health check endpoint `/health`
- [ ] SPA fallback para deep links del frontend

**Config hygiene:**
- [ ] URL del backend centralizada en frontend (environment files)
- [ ] JWT secret via env var, no en `appsettings.json`
- [ ] Admin seed via env vars (`ADMIN_EMAIL`/`ADMIN_PASSWORD`)
- [ ] `appsettings.Production.json` con config específica

**Data & sesiones:**
- [ ] EF Core migrations reemplazan `EnsureCreatedAsync`
- [ ] Sesiones en storage durable (o acepta pérdida documentada)

**Seguridad:**
- [ ] CORS con métodos explícitos (no `AllowAnyMethod`)
- [ ] Password mínimo 8 caracteres al registrar

**Observabilidad & DX:**
- [ ] Serilog con sinks console/file y request logging
- [ ] Polling frontend con backoff exponencial
- [ ] WordCategories desde JSON externo (no hardcoded en DbSeeder)
- [ ] Tests frontend habilitados (servicios clave)
- [ ] Tipos TypeScript generados desde API (NSwag) u OpenAPI

### Out of Scope

- WebSockets real-time — polling HTTP alcanza para el tamaño de partida (4-10 jugadores)
- Mobile app nativa — web-first
- Multi-idioma — todo en español, parte del diseño
- Pagos / monetización — juego personal/social

## Context

- **Usuario actual**: Mateo (dueño del proyecto) — reportó que "el proyecto anda mal y no anda ni siquiera el deploy"
- **Estado del deploy**: Render auto-deploya desde push a `main`. Actualmente roto (ver CONCERNS.md top 5)
- **Tests**: Backend 145 tests xUnit pasando en local (100% line / 99.19% branch). Frontend sin tests (`skipTests: true` en angular.json)
- **Arquitectura**: 3 proyectos .NET (`impojuego` core lib, `ImpoJuego.Api`, `ImpoJuego.Tests`) + Angular 17 standalone components
- **Idioma**: Todo el código, comentarios, UI y mensajes en español

## Constraints

- **Tech stack**: .NET 8 + Angular 17 + SQLite — establecido, no rehacer
- **Deploy target**: Render.com (free tier) — mantener
- **Auto-deploy**: Push a `main` dispara deploy — ambos servicios (backend + frontend)
- **Idioma**: Todo en español (código, comentarios, UI)
- **Tests backend**: 145 tests deben seguir pasando (no regresiones)

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Brownfield — saltar research/questioning | CLAUDE.md + CONCERNS.md ya tienen todo el contexto | — Pending |
| Fases coarse (5-6 fases) | 22 issues agrupados por dominio, mejor que 22 mini-fases | — Pending |
| Plan-check + verifier ON | Scope grande, quality gates valen la pena | — Pending |
| Research OFF | Stack conocido, no hay research útil que hacer | — Pending |
| Auto-deploy via git push | Vs. API de Render — más simple, atado al estado de git | — Pending |

---
*Last updated: 2026-04-13 after map-codebase*
