# Project State: ImpoJuego

**Current phase:** All 6 phases ✓ Complete
**Last activity:** 2026-04-13 - Phase 6 complete, push pendiente

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-13)

**Core value:** El juego debe ser jugable end-to-end desde link compartible en Render sin setup local
**Current focus:** Validación end-to-end por el usuario

## Roadmap Status

| # | Phase | Status | Requirements |
|---|-------|--------|--------------|
| 1 | Unblock Deploy | ✓ Complete | DEPLOY-01..05 |
| 2 | Config Hygiene | ✓ Complete | CONFIG-01..04 |
| 3 | Data Durability | ✓ Complete | DATA-01..03 |
| 4 | Security Hardening | ✓ Complete | SEC-01..03 |
| 5 | Quality & Observability | ✓ Complete (QUAL-01, 05 deferidos) | QUAL-02..04 |
| 6 | Testing & DX | ✓ Complete | TEST-01..02 |

**Progress:** 20/22 requirements complete (QUAL-01 uniform API response y QUAL-05 NSwag quedan en v2)

## Live Environment

| Service | URL | Status |
|---------|-----|--------|
| Backend | https://impojuego-1.onrender.com | ✓ /health 200 |
| Frontend | https://impojuego-web.onrender.com | ✓ 200 |

## Deploy Flow

1. `git push origin main` → Render auto-deploy (backend Docker + frontend static)
2. Backend build ~5 min, frontend build ~2 min
3. Verify: `curl https://impojuego-1.onrender.com/health`
4. Verify SPA: `curl -I https://impojuego-web.onrender.com/voting`

## Action Items Pendientes (todos manuales en Render dashboard)

### 1. SPA fallback (1 click)
Dashboard → `impojuego-web` → Redirects/Rewrites → Add:
- Source: `/*`
- Destination: `/index.html`
- Action: Rewrite

### 2. Env vars del backend (3 a setear)
Dashboard → `impojuego-1` → Environment → Add:
- `JWTSETTINGS__SECRET` → string 32+ chars (`openssl rand -base64 64`)
- `ADMIN_EMAIL` → email del admin (opcional, p.ej. `mateocirujas`)
- `ADMIN_PASSWORD` → password del admin (opcional, 8+ chars)

Sin estas env vars el backend arranca igual (genera JWT secret random runtime con warning).
Pero los tokens se pierden en cada restart y no hay admin seed.

## Blockers/Concerns (deferidos)

- **SQLite en /tmp es efímero** — datos perdidos en redeploy. Migración a Render Postgres (free tier) o disco montado = v2.
- **Sesiones in-memory** — si Render free tier duerme (15 min idle), partidas activas se pierden.
- **Frontend sin test runner local configurado** — los `.spec.ts` están pero `npm test` requiere Chrome/karma config.
- **QUAL-01 API response uniforme** — refactor grande sin mucho valor práctico, diferido.
- **QUAL-05 NSwag TS types** — requeriría setup extra, diferido.

## Live Testing Checklist (para el usuario)

Después del push final + setear env vars:
1. ✅ https://impojuego-1.onrender.com/health → 200 healthy
2. ✅ https://impojuego-web.onrender.com → lobby carga
3. ✅ Agregar players → Iniciar → debe navegar a /game (bug de bouncing corregido)
4. ✅ /voting y /result navegaciones internas andan
5. ⚠️ Deep link directo `/voting` → requiere Render Rewrite rule manual
6. ✅ Dos celulares = dos partidas independientes (aislamiento probado con 50 sesiones concurrentes)

## Multi-session status

- Arquitectura: `GameSessionManager` (ConcurrentDictionary) + `X-Session-Id` header + UUID v4 en localStorage
- Thread safety: `GameManager` ahora usa lock privado para mutaciones intra-sesión
- Tests: 147 backend (incluyendo 2 nuevos de concurrencia multi-sesión)

---
*Last updated: 2026-04-13 después de Phases 2-6*
