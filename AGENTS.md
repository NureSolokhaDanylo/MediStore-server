# MediStore Server Instructions

## Project Snapshot

MediStore Server is an ASP.NET Core backend for medicine storage management with SQL Server persistence, JWT auth, ASP.NET Identity users, IoT sensor readings, alerts, audit logs, and OpenAPI-based client generation.

## Solution Layout

- `WebApi/`: controllers, API DTOs, auth setup, OpenAPI transformers, startup wiring.
- `Application/`: business services, result types, hosted background workers, middleware, interfaces, seeders.
- `Infrastructure/`: EF Core `AppDbContext`, repositories, migrations, data-access implementations.
- `Entities/`: domain models.
- `ErrorContracts/`: shared machine-readable error codes.
- `SharedConfiguration/`: strongly typed options.
- `SensorsEmulator/`: local sensor emulator.
- `Scripts/`: docker, OpenAPI, and SDK helper scripts.

## Core Architectural Conventions

- Keep dependencies pointing inward: `WebApi -> Application -> Infrastructure -> Entities`.
- Application services should return `Result` / `Result<T>` instead of using exceptions for expected business failures.
- Controllers should convert failed results through the existing API error helpers instead of handcrafting error payloads.
- Data access should go through repositories and the current application/infrastructure abstractions, not ad hoc direct SQL from controllers.
- Access checks live in both the HTTP layer and the service layer. Do not rely only on controller attributes.

## Authentication And Roles

- JWT bearer auth is the default API auth scheme.
- Sensor ingestion uses `X-Sensor-Api-Key` and `RequireSensorApiKeyAttribute`.
- Main roles:
  - `Admin`: system management, settings, users, destructive changes.
  - `Operator`: operational write access for domain entities like batches.
  - `Observer`: read-oriented access.

## Error Contract Conventions

- The canonical error payload is `ApiError` with `code`, `message`, `status`, optional `traceId`, and optional `details`.
- Machine-readable business error codes live in `ErrorContracts/ErrorCodes.cs`.
- OpenAPI now documents internal error codes in each operation `description`, alongside auth requirements.
- OpenAPI no longer binds internal error codes to individual HTTP response descriptions.
- For controller metadata, prefer:
  - `[Authorize(...)]` or `[Authorize]` when needed
  - one `[ApiErrors(status1, status2, ..., Codes = new[] { ... })]`
- Do not reintroduce the old per-status `ApiErrorCodesAttribute` pattern.

## OpenAPI Workflow

- Runtime OpenAPI endpoints:
  - `/openapi/v1/openapi.json`
  - `/openapi/v1/openapi.yaml`
- The OpenAPI shaping logic lives in `WebApi/OpenApi/AuthOpenApiTransformers.cs`.
- The transformer is responsible for:
  - stable readable `operationId` values
  - auth security schemes
  - generic `ApiError` responses
  - operation descriptions with required roles / auth mode and internal error codes
  - schema normalization to avoid ugly generator artifacts

## Updating API Docs

- Use the project scripts, not ad hoc docker commands:
  - `./Scripts/lin/down.sh`
  - `./Scripts/lin/up_detached.sh`
  - `./Scripts/lin/get_api.sh`
- `Scripts/path_to_env` must contain the absolute path to the env file used by docker compose.
- `get_api.sh` and `Scripts/win/get_api.cmd` do two things on every successful fetch:
  - refresh root `openapi.json`
  - archive a new version in `OpenApiHistory/openapi.vNNNN.json`
- Root `openapi.json` is the compatibility entrypoint for SDK generation and other tooling.
- `OpenApiHistory/` is the local history of fetched specs and should be kept append-only.

## SDK Generation

- Temporary SDK generation scripts:
  - `./Scripts/lin/gen_sdk_ts.sh`
  - `./Scripts/lin/gen_sdk_kotlin.sh`
  - Windows equivalents under `Scripts/win/`
- Generated output locations:
  - `GeneratedClients/ts-fetch`
  - `GeneratedClients/kotlin`
- These SDKs are verification artifacts unless explicitly requested otherwise.
- After OpenAPI changes, smoke-check generated SDKs for:
  - readable operation names like `accountDelete`, `zonesDelete`, `medicinesDelete`
  - absence of integer-or-string / number-or-string union garbage in signatures
  - preserved `ApiError` model
  - representative docs still carrying operation descriptions with roles and internal codes
- Delete temporary generated SDKs after inspection unless the task explicitly says to keep them.

## Domain-Specific Constraints Worth Preserving

- Deleting a zone that still has batches must fail with `409 zone.has_batches`.
- Deleting a medicine that still has batches must fail with `409 medicine.has_batches`.
- These restrictions must stay aligned across:
  - service logic
  - database FK behavior
  - OpenAPI documentation

## Background Processing

Hosted services live in `Application/Hosted/` and include:

- `ExpiredChecker`
- `ExpirationSoonChecker`
- `BatchConditionChecker`
- `ZoneConditionChecker`
- `ReadingsRetentionCleaner`

When changing alerting or retention behavior, inspect both the background worker and `AppSettings`-driven configuration.

## Practical Editing Guidance

- If API behavior, DTOs, auth rules, OpenAPI metadata, or error contracts change, rebuild the dockerized API and fetch a fresh spec before considering the task done.
- If the fetched OpenAPI looks wrong, fix the transformer or controller metadata rather than hand-editing `openapi.json`.
- Keep controller annotations compact; avoid stacking multiple custom attributes when one will do.
- Prefer updating repo scripts if a workflow is repeated more than once manually.
