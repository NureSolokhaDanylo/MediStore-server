---
name: medistore-generate-sdk
description: Generate temporary TypeScript and Kotlin SDKs from the MediStore OpenAPI spec and inspect them for output quality in this repository. Use when openapi.json changes and you need to verify generated client names, model shapes, and obvious generator regressions before deleting the temporary SDK folders.
---

# MediStore Generate SDK

## Overview
Use this skill only inside this MediStore repository when a changed `openapi.json` needs temporary SDK generation and quick inspection.

## Workflow

1. Use root `openapi.json` by default unless the user explicitly requests another snapshot path.
2. Generate the TypeScript SDK with:
   - `./Scripts/lin/gen_sdk_ts.sh`
3. Generate the Kotlin SDK with:
   - `./Scripts/lin/gen_sdk_kotlin.sh`
4. Smoke-check the generated output before cleanup.
5. Delete the generated folders when inspection is complete:
   - `GeneratedClients/ts-fetch`
   - `GeneratedClients/kotlin`

## Smoke Checks

- Confirm both generator commands completed successfully.
- Inspect representative API files and confirm operation names are readable, for example `accountDelete`, `zonesDelete`, `medicinesDelete`.
- Inspect representative models and API docs for obvious generator regressions:
  - no ugly integer-or-string union artifacts in generated types
  - expected `ApiError` usage is still present
  - representative endpoints still mention important 4xx or 409 responses in generated docs or comments
- If output looks wrong, report the concrete artifact and likely OpenAPI cause instead of silently accepting the SDK.

## Rules

- Treat generated SDKs as temporary verification artifacts unless the user explicitly asks to keep them.
- Prefer the project scripts over raw `openapi-generator-cli` invocations so the workflow stays consistent.
- If the SDKs need to be checked against a freshly changed backend, run `$medistore-update-api` first.
