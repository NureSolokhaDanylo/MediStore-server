---
name: medistore-update-api
description: Update the MediStore backend API contract and refresh the local OpenAPI snapshots for this repository. Use when API endpoints, DTOs, authorization, OpenAPI metadata, error contracts, or controller behavior change and the repo needs a rebuilt docker service plus a fresh archived OpenAPI JSON and updated root openapi.json.
---

# MediStore Update API

## Overview
Use this skill only inside this MediStore repository when the API contract needs to be refreshed after backend changes.

## Workflow

1. Inspect the current API-related change and update the backend code first.
2. If the change affects controllers, DTOs, authorization, transformers, or error responses, rebuild the dockerized API through the project scripts.
3. Run the restart sequence exactly in this order:
   - `./Scripts/lin/down.sh`
   - `./Scripts/lin/up_detached.sh`
4. Wait briefly if the API is still starting, then fetch the spec with:
   - `./Scripts/lin/get_api.sh`
5. Confirm that the fetch created:
   - a new archived snapshot in `OpenApiHistory/openapi.vNNNN.json`
   - a refreshed root `openapi.json`

## Rules

- Prefer the repository scripts over ad hoc docker or curl commands.
- Treat root `openapi.json` as the compatibility entrypoint used by other scripts.
- Treat `OpenApiHistory/` as the append-only archive of fetched specs.
- If the API is unavailable right after startup, retry the fetch after a short wait instead of changing the workflow.
- If the task also asks for SDK verification, hand off to `$medistore-generate-sdk` after the new OpenAPI snapshot is fetched.
