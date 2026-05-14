# Copilot instructions for this repository

This file helps Copilot CLI sessions and future contributors quickly understand how to build, test, run, and navigate the project.

---

## Build, test, and lint commands

Web client (Angular, web-client/)
- Install: npm install (run in web-client/)
- Dev server: npm run start  (alias: ng serve)
- Build: npm run build  (alias: ng build)
- Run SSR server (after build): npm run serve:ssr:web-client
- Unit tests (Vitest): npm run test
  - Run a single test file or named test with vitest: npx vitest path/to/file.spec.ts or npx vitest -t "name pattern"
- Prettier formatting: project includes a Prettier config in package.json

Backend (.NET microservices, backend/)
- Build all services: dotnet build ./backend
- Run a single service: dotnet run --project ./backend/UserService
- Run database migrations (if using EF Core tools): dotnet ef database update --project ./backend/UserService --startup-project ./backend/UserService
- Run tests (if any): dotnet test ./path/to/testproject

Recommendation service (Python, backend/RecommendationService)
- Create venv & install: python -m venv .venv && .\.venv\Scripts\activate && pip install -r backend\RecommendationService\requirements.txt
- Run service locally: uvicorn main:app --host 0.0.0.0 --port 8000
- There is no test runner configured by default; use pytest if added.

Docker / full-stack
- Start full stack locally: docker-compose up --build (top-level backend/docker-compose.yml)
- Stop: docker-compose down

Notes:
- Use service-specific folders as working directories when running commands.
- Docker compose currently contains example secrets/API keys. Replace with env files or secrets manager before sharing.

---

## High-level architecture

- Monorepo split: web-client (Angular SSR + SPA) and backend (microservices)
- Backend microservices are primarily ASP.NET Core services living in backend/ (e.g., AuthService, UserService, ContentService, InteractionService), plus a Python-based RecommendationService.
- API Gateway (Ocelot) routes requests to internal services. Each service exposes HTTP endpoints on port 8080 inside the Docker network; docker-compose maps host ports (5000,5001, ...).
- Shared components:
  - PostgreSQL is the STRICT and ONLY datastore (Polyglot persistence - one DB per service). Do NOT suggest Redis or NoSQL.
  - RecommendationService (FastAPI) generates and writes recommendations into RecommendationSessions / RecommendedContents tables.
  - SQL schema files: backend\UserDb_Schema.sql and backend\InteractionDb_Schema.sql contain schema references useful for DB setup/migrations.
- Inter-service communication is via internal HTTP URLs defined in docker-compose environment variables (e.g., Services__User=http://user-service:8080).
- EF Core database migrations are triggered automatically at service startup in Program.cs for UserService (context.Database.Migrate()).

---

## Key repository conventions and patterns

- Frontend SSR Safety: The Angular app uses Server-Side Rendering. Never use `window`, `document`, or `localStorage` directly in component initialization. Always inject `PLATFORM_ID` and use `isPlatformBrowser()`.
- Cross-Service DTOs: Be extremely careful with JSON casing. C# uses PascalCase, while Python outputs camelCase/snake_case. Always use `[JsonPropertyName]` attributes in C# DTOs when communicating with the Python service.
- Per-service Dockerfiles and docker-compose are the canonical local-run method. Services expect ConnectionStrings__DefaultConnection environment variables.
- Environment variables follow ASPNETCORE and custom names (e.g., Jwt:Key in configuration). 
- Databases: Schema files live under backend/ for manual inspection. RecommendationService and services use direct SQL queries; check backend\RecommendationService\database.py for DB helper functions.
- Recommender pattern: Hybrid approach combining Content-Based Filtering (TF-IDF, Cosine Similarity) and Collaborative Filtering. The recommender loads content into a pandas DataFrame, computes matrices, and writes back top-N recommendations to DB via transaction.
- Logging and error handling: Services use Microsoft.Extensions.Logging and structured logs. Exceptions are intentionally rethrown to let controllers handle return codes.

---

## Repo-specific tips for Copilot sessions

- Focus searches on these folders: web-client/ backend/ backend\RecommendationService/
- Inspect docker-compose.yml early — it documents service names, internal ports, and required env variables.
- When suggesting code changes for services, prefer adjusting environment variables or Dockerfiles rather than hardcoding credentials.
- When producing SQL or migration changes, reference existing schema SQL files and run migrations locally (or via Docker) to validate.

---

## Where to look next

- web-client/README.md — Angular development notes and Vitest usage
- backend/docker-compose.yml — full local stack configuration and environment variables
- backend/RecommendationService/* — recommender implementation (recommender.py, main.py)
- backend/UserService/* — EF Core context and AuthService example