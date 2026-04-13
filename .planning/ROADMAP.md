# Roadmap: ImpoJuego

**Created:** 2026-04-13
**Milestone:** v1 — Deploy working + quality hardening

6 fases, 22 requirements, cobertura 100%.

## Phase Overview

| # | Phase | Goal | Requirements | Status |
|---|-------|------|--------------|--------|
| 1 | Unblock Deploy | Backend arranca en Render, frontend sirve rutas, DB persiste | DEPLOY-01..05 | Pending |
| 2 | Config Hygiene | Secrets/URLs fuera del repo, env-based | CONFIG-01..04 | Pending |
| 3 | Data Durability | Migrations + sesiones durables + datos desde archivo | DATA-01..03 | Pending |
| 4 | Security Hardening | CORS explícito, password policy, JWT audiencia correcta | SEC-01..03 | Pending |
| 5 | Quality & Observability | Logging, thread safety, API uniforme, polling inteligente | QUAL-01..05 | Pending |
| 6 | Testing & DX | Frontend tests + no regresiones backend | TEST-01..02 | Pending |

---

## Phase 1: Unblock Deploy

**Goal:** Conseguir que `git push` a `main` dispare un auto-deploy en Render que termine exitosamente y donde backend + frontend respondan en sus URLs de producción, incluyendo rutas profundas del frontend.

**Requirements:** DEPLOY-01, DEPLOY-02, DEPLOY-03, DEPLOY-04, DEPLOY-05

**Success Criteria:**
1. Push a `main` → backend y frontend se deploy-an sin error visible en logs de Render
2. `GET https://impojuego-1.onrender.com/health` devuelve `200 OK`
3. `https://impojuego-web.onrender.com/voting` carga el SPA (no 404)
4. Datos creados vía API persisten tras un redeploy manual
5. `render.yaml` en root commiteado y Render lo está usando

---

## Phase 2: Config Hygiene

**Goal:** Eliminar toda URL hardcoded y todo secret del código, moviéndolos a env vars de Render o archivos environment de Angular. Cambiar URL o secret no debe requerir tocar código.

**Requirements:** CONFIG-01, CONFIG-02, CONFIG-03, CONFIG-04

**Success Criteria:**
1. Grep de `onrender.com` en `impojuego-web/src/` retorna solo `environment.prod.ts`
2. Grep de `JWTSettings.Secret` o valores largos en `appsettings*.json` retorna vacío
3. Si Render no tiene `JWTSETTINGS__SECRET`, la app falla al arrancar con mensaje claro
4. Si no hay `ADMIN_EMAIL`/`ADMIN_PASSWORD`, la app arranca sin admin seed

---

## Phase 3: Data Durability

**Goal:** La base de datos evoluciona vía migrations y los datos sobreviven redeploys; sesiones de juego tienen política clara (persistidas o explícitamente efímeras).

**Requirements:** DATA-01, DATA-02, DATA-03

**Success Criteria:**
1. `dotnet ef migrations add <name>` funciona, carpeta `Migrations/` existe
2. `Program.cs` corre `Database.Migrate()` al arrancar
3. Palabras por categoría default leídas desde `Data/defaultCategories.json`
4. Sesiones sobreviven restart local (backed por SQLite/Postgres) o docs explican que se pierden

---

## Phase 4: Security Hardening

**Goal:** El servicio expuesto en internet no ofrece superficie innecesaria — CORS acotado, passwords con mínimo, JWT validado correctamente.

**Requirements:** SEC-01, SEC-02, SEC-03

**Success Criteria:**
1. `OPTIONS` con método `TRACE` o `CONNECT` recibe `405/Forbidden` desde origen permitido
2. `POST /api/auth/register` con password `"a"` devuelve `400` con mensaje claro
3. JWT emitido por otra app con audiencia distinta es rechazado

---

## Phase 5: Quality & Observability

**Goal:** Errores de producción son diagnosticables desde logs de Render; API responde con formato uniforme; el game loop no tiene race conditions ni auto-DoS por polling.

**Requirements:** QUAL-01, QUAL-02, QUAL-03, QUAL-04, QUAL-05

**Success Criteria:**
1. Serilog loggea cada request HTTP con status + duration
2. Cualquier error devuelto por API tiene la misma shape `{success, message, data}`
3. Tests concurrentes en `GameManager` no producen excepciones / estados inválidos
4. Frontend tras 3 errores consecutivos aumenta el intervalo de polling

---

## Phase 6: Testing & DX

**Goal:** Cambios futuros tienen red de seguridad en ambos lados y los 145 tests backend siguen verdes.

**Requirements:** TEST-01, TEST-02

**Success Criteria:**
1. `cd impojuego-web && npm test -- --watch=false --browsers=ChromeHeadless` corre al menos 10 tests y pasan
2. `cd impojuego && dotnet test` sigue reportando 145 tests, 0 fallos, coverage igual o mayor

---

*Roadmap created: 2026-04-13 from .planning/codebase/CONCERNS.md findings*
