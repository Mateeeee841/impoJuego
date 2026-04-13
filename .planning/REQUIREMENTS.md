# Requirements: ImpoJuego

**Defined:** 2026-04-13
**Core Value:** El juego debe ser jugable end-to-end desde link compartible en Render, sin setup local

## v1 Requirements

Cada requirement deriva de un issue concreto de `.planning/codebase/CONCERNS.md`.

### Deploy (DEPLOY)

- [x] **DEPLOY-01**: Backend lee `PORT` de env var y bindea a `0.0.0.0:$PORT` (CONCERNS #1)
- [x] **DEPLOY-02**: SQLite en path configurable via `DATABASE_PATH` env var (CONCERNS #2) — persistencia real deferred a Phase 3 (requiere disk o Postgres)
- [x] **DEPLOY-03**: `render.yaml` en root define backend (web service dotnet) y frontend (static site Angular) con auto-deploy desde `main` (CONCERNS #4) — nota: requiere "Sync blueprint" en Render dashboard para activar
- [x] **DEPLOY-04**: Endpoint `GET /health` responde `200 OK` con JSON `{"status":"healthy"}` (CONCERNS #22)
- [x] **DEPLOY-05**: Frontend estático sirve `index.html` como fallback para rutas no-API (SPA rewrite) (CONCERNS #5) — via `_redirects` file en dist/

### Config (CONFIG)

- [ ] **CONFIG-01**: URL del backend centralizada en `environment.ts` / `environment.prod.ts` y leída vía Angular environment (CONCERNS #3)
- [ ] **CONFIG-02**: JWT secret leído de env var `JWTSETTINGS__SECRET`, app aborta si falta en Production (CONCERNS #6)
- [ ] **CONFIG-03**: Admin seed condicionado a env vars `ADMIN_EMAIL` + `ADMIN_PASSWORD`, skippea si faltan (CONCERNS #8)
- [ ] **CONFIG-04**: `appsettings.Production.json` con log level Warning y settings prod-específicos (CONCERNS #12)

### Data (DATA)

- [ ] **DATA-01**: EF Core migrations reemplazan `EnsureCreatedAsync`, startup corre `Database.Migrate()` (CONCERNS #7)
- [ ] **DATA-02**: Sesiones persistidas (SQLite/Postgres-backed) o documentado que se pierden en redeploy (CONCERNS #16)
- [ ] **DATA-03**: WordCategories cargadas desde `Data/defaultCategories.json` en vez de dictionary hardcoded (CONCERNS #20)

### Security (SEC)

- [ ] **SEC-01**: CORS con `WithMethods("GET","POST","PUT","DELETE","OPTIONS")` — no `AllowAnyMethod` (CONCERNS #9)
- [ ] **SEC-02**: Password mínimo 8 caracteres validado en `AuthService.RegisterAsync` (CONCERNS #11)
- [ ] **SEC-03**: JWT audience/issuer validados correctamente con valores específicos (CONCERNS #10)

### Quality (QUAL)

- [ ] **QUAL-01**: `ApiResponse<T>` uniforme en success y error (estructura consistente) (CONCERNS #13)
- [ ] **QUAL-02**: Serilog configurado con sinks Console y request logging middleware (CONCERNS #14)
- [ ] **QUAL-03**: `GameManager` thread-safe vía lock en métodos mutantes o sesiones aisladas por lock (CONCERNS #18)
- [ ] **QUAL-04**: Frontend polling con backoff exponencial en `GameStateService` (CONCERNS #19)
- [ ] **QUAL-05**: Tipos TypeScript regenerados desde backend con NSwag o mantenidos en sync (CONCERNS #15)

### Testing (TEST)

- [ ] **TEST-01**: Frontend tests habilitados (`skipTests: false`), suite de smoke para `GameService`/`AuthService`/`SessionService` (CONCERNS #21)
- [ ] **TEST-02**: Backend tests siguen pasando al 100% line coverage (no regresión)

## v2 Requirements

Deferred — no bloquean funcionalidad ni deploy.

- **OBS-01**: Sinks de Serilog a archivo con rotación (Serilog.Sinks.File)
- **OBS-02**: Dashboard de métricas (Prometheus/Grafana)
- **SEC-04**: Password hashing con argon2id (sobre BCrypt)
- **DATA-04**: Migración completa a PostgreSQL con pool de conexiones

## Out of Scope

| Feature | Reason |
|---------|--------|
| WebSockets real-time | Polling alcanza para 4-10 jugadores por sesión |
| Mobile app nativa | Web-first, responsive alcanza |
| Multi-idioma | Todo en español es parte del diseño del producto |
| OAuth (Google/GitHub) | Email+password es suficiente para v1 |
| Pagos/monetización | Proyecto personal, fuera de alcance |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| DEPLOY-01 | Phase 1 | Complete |
| DEPLOY-02 | Phase 1 | Complete (partial - disk persistence deferred) |
| DEPLOY-03 | Phase 1 | Complete (blueprint not synced in Render yet) |
| DEPLOY-04 | Phase 1 | Complete |
| DEPLOY-05 | Phase 1 | Complete |
| CONFIG-01 | Phase 2 | Pending |
| CONFIG-02 | Phase 2 | Pending |
| CONFIG-03 | Phase 2 | Pending |
| CONFIG-04 | Phase 2 | Pending |
| DATA-01 | Phase 3 | Pending |
| DATA-02 | Phase 3 | Pending |
| DATA-03 | Phase 3 | Pending |
| SEC-01 | Phase 4 | Pending |
| SEC-02 | Phase 4 | Pending |
| SEC-03 | Phase 4 | Pending |
| QUAL-01 | Phase 5 | Pending |
| QUAL-02 | Phase 5 | Pending |
| QUAL-03 | Phase 5 | Pending |
| QUAL-04 | Phase 5 | Pending |
| QUAL-05 | Phase 5 | Pending |
| TEST-01 | Phase 6 | Pending |
| TEST-02 | Phase 6 | Pending |

**Coverage:**
- v1 requirements: 22 total
- Mapped to phases: 22
- Unmapped: 0 ✓

---
*Requirements defined: 2026-04-13*
*Last updated: 2026-04-13 after map-codebase*
