# GameOfLifeApi (ASP.NET Core 7 + LiteDB)

Production-ready REST API for Conway's Game of Life, built with .NET 7 and persisted with LiteDB.

## What's the "Game of Life", by Conway's?
Conway's Game of Life is a cellular automaton devised by the British mathematician John Horton Conway in 1970. It is a zero-player game, meaning that its evolution is determined by its initial state, requiring no further input. One interacts with the Game of Life by creating an initial configuration and observing how it evolves.

## Rules
- Any live cell with fewer than two live neighbours dies, as if by underpopulation.
- Any live cell with two or three live neighbours lives on to the next generation.
- Any live cell with more than three live neighbours dies, as if by overpopulation.
- Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

## Features
- Upload a board, query current/next states, jump N steps, advance and persist, compute final state/loop.
- ASP.NET Core 7, DI, LiteDB file persistence (App_Data/GameOfLife.db), Swagger (Development), xUnit + FluentAssertions.

## Requirements / Requisitos
- .NET SDK 7.0
- Optional/Opicional: Docker + Docker Compose

## Run locally / Executar localmente
```
cd GameOfLifeApi
 dotnet restore
 dotnet run
```
Swagger is enabled in Development at https://localhost:5001/swagger (or http://localhost:5000/swagger).

## Run with Docker
```
docker compose up --build gameoflifeapi
```
The API listens on http://localhost:8080 (Swagger at /swagger).

Data persistence: LiteDB at App_Data/GameOfLife.db (mounted volume in Docker).

## REST API
Base: /api/boards

### Endpoints Table

| Endpoint | Required parameters | Request | Response | Possible errors | Description |
|---|---|---|---|---|---|
| POST /api/boards | Body: grid (rectangular bool[][]) | POST application/json. Body: { "grid": bool[][] } | 200 OK: GUID (string) | 400 BadRequest: { "message": "<validation error>" } | Creates a new board persisted in LiteDB; generation = 0 |
| GET /api/boards/{id} | Path: id (GUID) | GET | 200 OK: BoardStateResponse { id, generation, width, height, aliveCount, grid } | 404 NotFound | Returns current state without mutation |
| GET /api/boards/{id}/next | Path: id (GUID) | GET | 200 OK: BoardStateResponse | 404 NotFound | Computes next state (no persistence, does not change generation) |
| GET /api/boards/{id}/steps/{n} | Path: id (GUID), n (int >= 0) | GET | 200 OK: BoardStateResponse (generation = current + n) | 400 BadRequest: { "message": "n must be >= 0" } 404 NotFound | Returns state after N steps (no persistence) |
| POST /api/boards/{id}/advance?steps={s} | Path: id (GUID); Query: steps (int >= 0, default=1) | POST (no body) | 200 OK: BoardStateResponse (persisted) | 400 BadRequest: { "message": "steps must be >= 0" } 404 NotFound | Advances and persists the board by S steps |
| GET /api/boards/{id}/final?maxAttempts={m} | Path: id (GUID); Query: maxAttempts (int, default=10000) | GET | 200 OK: FinalStateResponse { id, finalGrid, stepsTaken, isLoop, period, conclusion } | 404 NotFound; 422 UnprocessableEntity: { "message": "<reason>" } | Attempts to conclude (stable/loop) within maxAttempts |

1) POST /api/boards — Upload board
Body (JSON):
```
{
  "grid": [[true, false, true], [false, true, false]]
}
```
Response: 200 OK with GUID (board Id).

2) GET /api/boards/{id} — Current state
3) GET /api/boards/{id}/next — Next state (does not mutate)
4) GET /api/boards/{id}/steps/{n} — State after N steps (n >= 0, no mutation)
5) POST /api/boards/{id}/advance?steps=1 — Advance and persist (steps >= 0)
6) GET /api/boards/{id}/final?maxAttempts=10000 — Compute conclusion (stable/loop) or 422 if none within attempts

Example cURL:
```
# Upload
curl -s POST https://localhost:5001/api/boards \
 -H "Content-Type: application/json" \
 -d '{"grid": [[true,false],[false,true]]}'

# Get current
curl -s https://localhost:5001/api/boards/{id}
```

## Project structure
- Controllers: BoardsController
- Services: GameOfLifeService (Next, Step, ComputeConclusion)
- Repositories: LiteDbBoardRepository (LiteDB file in App_Data)
- DTOs: UploadBoardRequest, BoardStateResponse, FinalStateResponse

## Testing & Coverage
Local:
```
cd GameOfLifeApi.Tests
 dotnet test                      # all tests
 dotnet test --collect:"XPlat Code Coverage"  # with coverage
```
CI (GitHub Actions): <mcfile name="tests-ci.yml" path="/Users/alvaropaco/github/GameOfLifeApi/.github/workflows/tests-ci.yml"></mcfile>
- Runs unit and integration tests on push/PR to main.
- Publishes test results to PR and posts a sticky coverage comment.
- Enforces minimum 80% coverage (fails below 80%).

## Notes
- Swagger only in Development; Docker compose sets Development and exposes /swagger.
- Data survives restarts via LiteDB file at App_Data/GameOfLife.db (volume in Docker).
