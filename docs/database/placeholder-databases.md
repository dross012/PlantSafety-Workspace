# Placeholder Databases

**Date:** 2026-07-18

The following 8 databases exist on `.\SQLEXPRESS` as part of the PlantSafety database-per-solution
estate but currently have **no Domain-layer source material** backing them. Per an explicit user
decision (Option A), these are being left as empty shells rather than having their schemas guessed
or invented. This document — together with a matching `MS_Description` database-level extended
property set live on each database — encodes that decision explicitly so it is not silently
implicit.

## Rationale (applies to all 8 databases below)

> Placeholder for future bounded context. No domain aggregates exist yet; no schema is deployed.
> Do not populate without a corresponding Domain-layer module.

Each database was verified live to have this exact text set as its `MS_Description` extended
property (`sys.extended_properties`, `class = 0`, database-level scope) via an idempotent
check-then-add/update script.

## Databases

| Database | Status |
|---|---|
| `PlantSafety.MaintenanceDB` | Empty shell — placeholder |
| `PlantSafety.NotificationsDB` | Empty shell — placeholder |
| `PlantSafety.ProductionDB` | Empty shell — placeholder |
| `PlantSafety.RoutingDB` | Empty shell — placeholder |
| `PlantSafety.SandboxDB` | Empty shell — placeholder |
| `PlantSafety.SchedulingDB` | Empty shell — placeholder |
| `PlantSafety.ToolingDB` | Empty shell — placeholder |
| `PlantSafety.TestHarnessDB` | Empty shell — placeholder |

## Next steps

Before any schema is deployed to one of these databases, the corresponding module must first have
a real Domain-layer source tree (aggregates/entities/value objects) under
`C:\Users\dross\source\<Module>\**\Domain\**`, per the Schema Sentinel's "ground every script in
real Domain-layer source, never invent fields" rule. Once that source material exists, re-run
Schema Sentinel against that module to generate and validate the canonical schema, and remove this
database from the placeholder list.
