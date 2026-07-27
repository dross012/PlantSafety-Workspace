# Step B — RowVersion → NVARCHAR Conversion Fix Plan (CorrectiveAction / ProcessManagement)

**Status note:** a background session (task `task_9afdf7ce`) was already
dispatched to apply this fix and, as of this writing, has already applied
it to all four target files (confirmed by direct inspection — see section
3). This document formalizes the correction as a specification/rollout
plan and does not itself modify anything.

## 1. The defect

SQL Server rejects a direct `CONVERT`/`CAST` from a `rowversion`
(`timestamp`) column to a character type, **even when an explicit
conversion style code is supplied**:

```sql
-- Throws: "Explicit conversion from data type timestamp to nvarchar is not allowed."
SELECT CONVERT(NVARCHAR(34), RowVersion, 1) AS ETag FROM ...
```

This is a documented SQL Server restriction, not a version-specific quirk
— `rowversion`/`timestamp` is treated as a distinct type from
`binary`/`varbinary` for `CAST`/`CONVERT` purposes, despite being
binary-compatible. Confirmed live against a real SQL Server 2022 instance
(Testcontainers) while adding integration test coverage for
`IncidentManagement` — this was not a theoretical finding.

**The fix**: cast through `BINARY(8)` first, which SQL Server does permit
converting to a character type:

```sql
CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
```

This produces an identical output string to what the broken form was
*intended* to produce (a 34-character `0x`-prefixed hex string) — it is a
pure bug fix, not a format change. Any caller comparing this string
against a previously-issued ETag (e.g. `usp_Incident_Update`'s
`WHERE ... AND CONVERT(...) = @ETag`) is unaffected in terms of format,
only in that the statement now executes instead of throwing.

## 2. Why this affects CorrectiveAction and ProcessManagement specifically

The pattern originates from `ProcessManagement.usp_Process_GetById`,
explicitly cited as the precedent in comments in `usp_Incident_Update.sql`
and `usp_IncidentRead_GetById.sql` ("matching the concurrency-token
pattern... ProcessManagement.usp_Process_GetById"). Because it was copied
forward as an established convention, it propagated to every module that
adopted ROWVERSION-based optimistic concurrency after `ProcessManagement`
did, including `ContinuousImprovement.CorrectiveAction` (CAPA) — added in
the Phase 2 aggregate redesign — and was never caught because, per this
session's own findings (see the Step C plan), the integration test suite
that would exercise these procedures against a real SQL Server has not
been successfully run end-to-end during this modernization effort.

## 3. Exact scope — all four target files, current state confirmed

Verified by direct inspection at the time of writing this plan (not
assumed):

| File | Line | Status |
|---|---|---|
| `PlantSafety.Infrastructure/ContinuousImprovement/Sql/Procedures/usp_CorrectiveAction_GetById.sql` | 47 | **Fixed** — `CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag` |
| `PlantSafety.Infrastructure/ContinuousImprovement/Sql/Procedures/usp_CorrectiveAction_List.sql` | 55 | **Fixed** |
| `PlantSafety.Infrastructure/ProcessManagement/Sql/Procedures/usp_Process_GetById.sql` | 30 | **Fixed** — uses bracketed `[RowVersion]`/`[ETag]`, same fix applied |
| `PlantSafety.Infrastructure/ProcessManagement/Sql/Procedures/usp_Process_List.sql` | 37 | **Fixed** |

A repo-wide grep for the broken pattern
(`CONVERT(NVARCHAR..., RowVersion, ...)` without an intervening `CAST`)
across every `.sql` and `.cs` file returns **zero remaining matches** as
of this writing — the fix, combined with the six `SafetyEvents.Incident`
procedures and `TestDatabaseFactory.cs`'s embedded schema copies fixed
earlier in this session, appears to be complete across the codebase.

**One gap this plan flags rather than resolves**: `TestDatabaseFactory.cs`
(the Testcontainers-backed integration test schema setup) has embedded
inline copies of the `SafetyEvents.Incident` procedures, which is why the
Incident-side bug was catchable by an integration test at all. It has
**no embedded copies of the CorrectiveAction or ProcessManagement
procedures** — meaning there is currently no integration test path that
applies these four fixed procedures to a real database and exercises
them. If `CAPAIntegrationTests.cs` or a ProcessManagement integration
test exists and runs against the real `.sql` files directly (rather than
an embedded copy), it may already provide coverage; if it relies on
`TestDatabaseFactory` the same way `IncidentLifecycleIntegrationTests.cs`
does, it will need the same kind of embedded-schema addition before it
can prove this fix live. This determination was not made as part of this
plan — verify which case applies before assuming test coverage exists.

## 4. Rollout plan

1. **Confirm the fix compiles/parses** — SQL syntax has no build step in
   this codebase's normal `dotnet build` (the `.sql` files aren't
   compiled artifacts), so validate by applying the four procedures to a
   scratch/test SQL Server instance and confirming `CREATE OR ALTER
   PROCEDURE` succeeds without error. (Already implicitly done: the
   equivalent `SafetyEvents.Incident` fix was proven this way earlier in
   this session — schema application, which previously failed at this
   exact statement shape, now succeeds.)
2. **Verify each procedure's behavior is unchanged for the non-broken
   path** — since these procedures likely also do useful work beyond the
   ETag projection (returning other CorrectiveAction/Process columns),
   confirm no other logic in the same procedure body was altered.
3. **Add integration coverage** if the gap in section 3 turns out to be
   real — extend `TestDatabaseFactory.cs` with embedded schema for
   `ContinuousImprovement.CorrectiveAction` and/or
   `ProcessManagement.Process` (following the exact pattern already used
   for `SafetyEvents.Incident`), then confirm
   `CAPAIntegrationTests.cs`/the ProcessManagement equivalent exercises a
   `GetById`/`List` call and successfully receives a non-throwing,
   correctly-shaped ETag string.
4. **No downstream consumer changes needed** — this is a pure
   bug-to-correct-behavior fix at the SQL layer; no C# code should need
   to change, since the ETag string format and column contract are
   unchanged, only whether the statement executes without error.

## 5. Verification steps (for whoever runs this, deferred per this task's "no execution" constraint)

- Build `PlantSafety.Infrastructure` and `PlantSafety.IntegrationTests`
  cleanly (`dotnet build ... -tl:off` — `-tl:off` avoids the MSBuild
  terminal-logger output-buffering issue observed earlier in this
  session).
- Run the relevant integration tests via the native xUnit v3 executable
  (`PlantSafety.IntegrationTests.exe`, **not** `dotnet test`, which
  produces no usable console output in this environment) — Docker must
  be running for Testcontainers to provision a real SQL Server instance.
- Confirm zero SQL exceptions during schema setup and zero
  ETag-projection failures during test execution, for both
  CorrectiveAction and ProcessManagement test classes.
- This work is currently blocked from full end-to-end verification by
  the unrelated `ICultureRepository` DI gap (Step A / Step C) — the
  shared `WebApplicationFactory` host must build before any integration
  test in this project can run at all.
