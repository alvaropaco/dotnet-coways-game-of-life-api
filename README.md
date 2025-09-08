# Game of Life API

[![.NET](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet&logoColor=white)](#)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet&logoColor=white)](#)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](#)
[![License: GPL v3](https://img.shields.io/badge/License-GPL%203.0-blue.svg)](LICENSE)
[![Coverage ≥ 80%](https://img.shields.io/badge/coverage-%E2%89%A5%2080%25-brightgreen.svg)](#)
<!-- Optionally add your CI badge below by replacing OWNER/REPO -->
<!-- [![CI - Unit and Integration Tests](https://github.com/OWNER/REPO/actions/workflows/tests-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/tests-ci.yml) -->

## 1. Project Title
Game of Life API — A production-ready .NET 7 Web API for Conway’s Game of Life.

## 2. Description
This repository contains a production-ready RESTful API that implements Conway’s Game of Life. Built with ASP.NET Core 7 and persisted with LiteDB, it follows a clean, testable architecture with layered design (Controllers → Services → Repositories → DTOs) and continuous integration. The API supports creating boards, computing next/step states, advancing/persisting generations, and detecting final/loop states.

## 3. Key Features
- Create a board and persist it in LiteDB (embedded, file-backed)
- Query current and next states without mutation
- Jump N steps ahead deterministically (no persistence) or advance and persist state
- Compute final state/loop with period detection
- CORS-enabled for local frontend development
- Swagger UI in Development environment
- Unit and integration tests with coverage; CI reports coverage (target ≥80%)

## 4. Technology Stack
- Language/Runtime: .NET 7 (C#)
- Web: ASP.NET Core Web API
- Persistence: LiteDB (embedded NoSQL, file-backed)
- Documentation: Swagger (Swashbuckle)
- Testing: xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing, Coverlet (collector)
- CI: GitHub Actions (unit + integration, coverage summary)
- Containerization: Docker and Docker Compose

## 5. Installation Instructions
Prerequisites:
- .NET SDK 7.0
- Optional: Docker + Docker Compose

Local run (Development):
```bash
cd GameOfLifeApi
 dotnet restore
 dotnet run
```
- Swagger (Development): https://localhost:5001/swagger (or http://localhost:5000/swagger)

Run with Docker:
```bash
# Build and start the API on port 8080
docker compose up --build gameoflifeapi
```
- API base: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Data persistence: LiteDB file mounted at App_Data/GameOfLife.db (via volume)

## 6. Configuration
Environment variables:
- ASPNETCORE_ENVIRONMENT: Development (default in Docker) or Production
- ASPNETCORE_URLS: Host binding (Docker compose sets http://0.0.0.0:8080)
- DOTNET_RUNNING_IN_CONTAINER: Used by the app to avoid HTTPS redirection inside containers

CORS:
- The API defines a named policy AllowFrontend in Program.cs
- Default allowed origins (local dev):
  - http://localhost:5178, http://localhost:5179, http://localhost:3000
- To change origins, update Program.cs and rebuild

Data directory:
- By default, data is stored at App_Data/GameOfLife.db under the app base path
- Docker composes a volume to persist data across restarts

## 7. Usage Examples
You can interact with the API via Swagger UI or cURL.

Create (upload) a board:
```bash
# Development (HTTPS)
curl -s -X POST https://localhost:5001/api/boards \
  -H "Content-Type: application/json" \
  -d '{"grid": [[true,false,true],[false,true,false]]}'

# Docker (HTTP)
curl -s -X POST http://localhost:8080/api/boards \
  -H "Content-Type: application/json" \
  -d '{"grid": [[true,false],[false,true]]}'
```

Get current state:
```bash
curl -s http://localhost:8080/api/boards/{id}
```

Get next (no persistence):
```bash
curl -s http://localhost:8080/api/boards/{id}/next
```

Get N steps ahead (n ≥ 0, no persistence):
```bash
curl -s http://localhost:8080/api/boards/{id}/steps/{n}
```

Advance and persist (steps ≥ 0, default=1):
```bash
curl -i -X POST "http://localhost:8080/api/boards/{id}/advance?steps=2"
```

Update the board grid (replace cells):
```bash
curl -i -X PUT http://localhost:8080/api/boards/{id} \
  -H "Content-Type: application/json" \
  -d '{"grid": [[true,true],[true,true]]}'
```

Get final/loop conclusion (422 if not concluded within attempts):
```bash
curl -s "http://localhost:8080/api/boards/{id}/final?maxAttempts=10000"
```

## 8. API Endpoints Documentation
Base path: `/api/boards`

| Endpoint | Method | Params | Request | Response | Errors | Description |
|---|---|---|---|---|---|---|
| `/api/boards` | POST | — | JSON `{ "grid": bool[][] }` | `200 OK` GUID (string) | `400 BadRequest` `{ message }` | Create and persist a new board; generation = 0 |
| `/api/boards/{id}` | GET | id: GUID | — | `200 OK` `BoardStateResponse` | `404 NotFound` | Current state (no mutation) |
| `/api/boards/{id}/next` | GET | id: GUID | — | `200 OK` `BoardStateResponse` | `404 NotFound` | Next state (no persistence) |
| `/api/boards/{id}/steps/{n}` | GET | id: GUID, n: int ≥ 0 | — | `200 OK` `BoardStateResponse` (generation = current + n) | `400 BadRequest` (invalid n), `404 NotFound` | State after N steps (no persistence) |
| `/api/boards/{id}/advance?steps={s}` | POST | id: GUID, steps ≥ 0 | — | `200 OK` `BoardStateResponse` (persisted) | `400 BadRequest`, `404 NotFound` | Advance and persist the board by S steps |
| `/api/boards/{id}` | PUT | id: GUID | JSON `{ "grid": bool[][] }` | `200 OK` `BoardStateResponse` | `400 BadRequest`, `404 NotFound` | Replace current grid (does not change generation) |
| `/api/boards/{id}/final?maxAttempts={m}` | GET | id: GUID, m: int | — | `200 OK` `FinalStateResponse` | `404 NotFound`, `422 UnprocessableEntity` | Attempt to conclude (stable/loop) within m attempts |

DTOs:
- Upload: `{ grid: bool[][] }` (rectangular, non-empty)
- BoardStateResponse: `{ id, generation, width, height, aliveCount, grid }`
- FinalStateResponse: `{ id, finalGrid, stepsTaken, isLoop, period, conclusion }`

## 9. Testing
Local testing:
```bash
cd GameOfLifeApi.Tests
 dotnet test                             # all tests
 dotnet test --collect:"XPlat Code Coverage"  # with coverage
```

Continuous Integration:
- Workflow: [.github/workflows/tests-ci.yml](.github/workflows/tests-ci.yml)
- Runs unit and integration test jobs
- Publishes test results to PRs and generates a coverage summary
- Generates a coverage summary comment; no hard coverage threshold is enforced by CI

## 10. Deployment
Docker (recommended for local and simple deployments):
```bash
# Build the image
docker compose build gameoflifeapi

# Run the container
docker compose up -d gameoflifeapi

# Logs
docker logs -f gameoflifeapi
```
- The API listens on `http://localhost:8080`
- Data persisted under `App_Data` via named volume

Kubernetes / Cloud:
- Build and publish a container image to your registry
- Create a Deployment and Service (LoadBalancer/Ingress) mapping container port 8080
- Configure environment variables and persistent storage as needed

## 11. Why This is Production-Ready
- Architecture & Maintainability
  - Layered design (Controllers → Services → Repositories → DTOs) with DI and clear separation of concerns
  - Strong typing and validation for input (rectangular grid checks)
- Scalability
  - Stateless HTTP API suitable for horizontal scaling
  - Repository abstraction enables migration from LiteDB to external databases without API changes
- Security
  - HTTPS redirection enabled by default outside containers
  - CORS policy restricts allowed origins; preflight handling in middleware
  - Clean 4xx/5xx error semantics (400 on validation, 404 when resources are missing, 422 for non-conclusive finals)
- Performance
  - Efficient grid processing (tight loops, minimal allocations)
  - Lightweight stack (ASP.NET Core) with fast startup
- Reliability & Quality
  - Comprehensive unit and integration tests via WebApplicationFactory
  - CI pipeline with coverage reporting and PR feedback
  - Idempotent, side-effect-free reads; explicit mutation endpoints

## 12. License Information
This project is licensed under the GNU General Public License v3.0 — see the [LICENSE](LICENSE) file for details.
