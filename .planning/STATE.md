# Project State: ImpoJuego

**Current phase:** Phase 1 — Unblock Deploy (✓ Complete)
**Next phase:** Phase 2 — Config Hygiene
**Last activity:** 2026-04-13 - Phase 1 complete, deploy working, /health responds 200

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-13)

**Core value:** El juego debe ser jugable end-to-end desde link compartible en Render sin setup local
**Current focus:** Phase 2 — Config Hygiene (siguiente)

## Roadmap Status

| # | Phase | Status | Requirements |
|---|-------|--------|--------------|
| 1 | Unblock Deploy | ✓ Complete | DEPLOY-01..05 |
| 2 | Config Hygiene | ○ Pending | CONFIG-01..04 |
| 3 | Data Durability | ○ Pending | DATA-01..03 |
| 4 | Security Hardening | ○ Pending | SEC-01..03 |
| 5 | Quality & Observability | ○ Pending | QUAL-01..05 |
| 6 | Testing & DX | ○ Pending | TEST-01..02 |

**Progress:** 5/22 requirements complete

## Live Environment

| Service | URL | Status |
|---------|-----|--------|
| Backend | https://impojuego-1.onrender.com | ✓ /health 200 |
| Frontend | https://impojuego-web.onrender.com | ✓ 200 (SPA fallback desplegándose tras commit `9566d2f`) |

## Deploy Flow (cómo monitorear)

1. `git push origin main` → Render auto-deploy
2. Backend Docker build ~5 min (SDK pull + restore + publish)
3. Static site build ~1-2 min (npm ci + ng build)
4. Verify: `curl https://impojuego-1.onrender.com/health`
5. Verify SPA: `curl -I https://impojuego-web.onrender.com/voting` (debe ser 200, no 404)

## Blockers/Concerns

- **SPA fallback requiere paso manual en Render**: el deep link `/voting`, `/game`, `/result` devuelve 404 hasta que se haga UNA de estas opciones:
  - **A** (1 min, UI): Dashboard → `impojuego-web` → Redirects/Rewrites → Add rule `Source: /*  Destination: /index.html  Action: Rewrite`
  - **B** (requiere conectar blueprint): Dashboard → Blueprints → Sync → aplica `routes` de `render.yaml`
  - Render NO soporta archivo `_redirects` estilo Netlify (se probó y no aplica).
- **Data persistence**: `/tmp/impojuego.db` es efímero. Phase 3 debe elegir: Render disk (paid) o Postgres free tier.
- **render.yaml no sincronizado**: cambios de env vars o servicios nuevos requieren sync manual en dashboard (Blueprints → Sync).
- **Sesiones in-memory**: si el backend se recicla (free tier duerme tras 15 min idle), partidas activas se pierden. Phase 5/3.

## Quick Tasks Completed

(none yet — planning/execution flowed through phases)

## Multi-session status (verificado 2026-04-13)

- Arquitectura: `GameSessionManager` (ConcurrentDictionary) + `X-Session-Id` header + UUID v4 en localStorage
- Aislamiento: verificado con 19 tests (2 nuevos: 50 sesiones concurrentes + race en GetOrAdd)
- Uso típico: cada dispositivo = 1 partida independiente. Confirmado que estado no se contamina entre sesiones.

---
*Last updated: 2026-04-13 after Phase 1 deploy*
