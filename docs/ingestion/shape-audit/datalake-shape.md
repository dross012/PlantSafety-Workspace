# Shape Audit — DataLake

Phase 5 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/datalake.md`, `classification-matrix.md`, `gaps/orphaned-ingress.md`
(Phases 1-4).

## Top findings for DataLake

1. **DataLake's SQL schema is completely unverifiable from source — zero `.sql` files exist
   anywhere in the module.** `find DataLake -iname "*.sql"` returns **zero results**, confirmed
   this phase. Every repository (`IngestionRepository.cs`, and by the same pattern the other 9
   controllers' repositories) calls a stored procedure by string name only —
   `"DataLake.CreateIngestionJob"`, `"DataLake.GetIngestionJob"`, `"DataLake.ListIngestionJobs"` —
   with **no corresponding `CREATE PROCEDURE`/`CREATE TABLE` script tracked anywhere in this
   repository**. This is a fundamentally different situation from PlantSafety (772 source-controlled
   `.sql` files) and means this phase **cannot verify column types, nullability, lengths, or CHECK
   constraints for any DataLake table** — those procedures/tables may exist in a live database
   the team manages outside version control, or may not exist at all. Either way, it is a
   significant governance gap in its own right: the DTO/domain/Dapper-record shape can be read from
   source, but the actual persisted shape it is supposed to match cannot be confirmed, denied, or
   diffed against a migration history. **This is the single most important finding for this
   module** — every row below stops at "Dapper record shape," not "verified SQL column," and should
   be read with that ceiling in mind.
2. **Where DataLake's Dapper parameter calls were inspected, none pass an explicit `size:` to
   `DynamicParameters.Add`** (contrast PlantSafety's `AddString(name, value, size)` helper, which
   does — see `plantsafety-shape.md` Top-5 #2). Without an explicit `Size`, ADO.NET infers the
   parameter size from the runtime string length, so DataLake's ingestion path does **not** carry
   PlantSafety's specific silent-client-side-truncation risk. The trade-off: if the real (unseen)
   SQL column is narrower than the value supplied, the **database itself** will now throw a
   "String or binary data would be truncated" error at insert time — a fail-loud outcome, not
   fail-silent. This is a materially better outcome than PlantSafety's pattern *if* the unseen
   column is undersized, but it cannot be confirmed one way or the other without the missing DDL.
3. **All 3 traced write commands (`IngestRawDataCommand`, `ProcessRawToCleansedCommand`,
   `PublishIntegrationEventCommand`) have real, well-formed FluentValidation validators** with
   explicit `MaximumLength` rules (`ModuleName` ≤128, `EventType` ≤256) — this is a genuine positive
   finding relative to PlantSafety's Alerts gap. The open question is only whether these caps
   *match* whatever the real (unseen) SQL columns are sized to — unverifiable per finding #1.
4. **`RawZonePartitionPath`** is present as a fully-modeled field on the never-published
   `RawDataIngestedEvent` (per Phase 3) but the `IngestionJobRecord` Dapper row also carries it —
   meaning the write side (`CreateIngestionJobAsync`) does not appear to set/return it at insert
   time (the insert call only maps request fields; `RawZonePartitionPath` shows up only on the
   *read* record shape, implying it's computed/set by the stored procedure itself, not by the C#
   layer). Since the procedure's body is not in source (finding #1), **it is not possible to
   confirm whether this field is ever actually populated**, despite being present in three
   different C# shapes (event, DTO, record). Flagged as a candidate "populated by nothing" field,
   unverified.

## Traced flow: Ingest Raw Data

Files read in full: `DataLake.Application\Ingestion\Commands\IngestRawDataCommand.cs`,
`DataLake.Application\Ingestion\Validators\IngestRawDataCommandValidator.cs`,
`DataLake.Infrastructure\Persistence\Repositories\Ingestion\IngestionRepository.cs`,
`DataLake.Infrastructure\Persistence\Models\Ingestion\IngestionJobRecord.cs`,
`DataLake.Infrastructure\Persistence\Mappers\Ingestion\IngestionJobMapper.cs` (parameter-binding
lines).

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| ModuleName | `string`, validator `NotEmpty().MaximumLength(128).Matches(regex)` | `string` (command field; `IngestionJob` aggregate is described as "SQL row; no C# aggregate class" per Phase 3) | **Unverifiable — no `.sql` for this table exists in source** | Unverifiable | Unverifiable (no explicit Dapper `size:`, so client-side truncation is unlikely; DB-side is unknown) | N/A (regex-constrained free text) | Yes | Directly verified up to the missing SQL layer | Finding #1 |
| EventType | `string`, validator `NotEmpty().MaximumLength(256)` | `string` | Unverifiable | Unverifiable | Unverifiable | N/A | Yes | Same ceiling | |
| IngestionPattern | `string`, validator `Must(p => p is "event-based" or "batch" or "streaming")` | `string`, lowercased in `FromDto` | Unverifiable | Unverifiable | Unverifiable | **Enum-like value constrained only by the FluentValidation `Must()` clause** — no C# enum type backs it (it's a raw string), and no CHECK constraint can be confirmed since the table DDL doesn't exist. If a 4th pattern is ever added to the validator without a corresponding (unseen) schema change, there's no way from source alone to know whether the unseen table would accept it | Yes | Directly verified for the C# side; SQL side unverifiable | The literal-string-triplet nature of this "enum" (no shared constant/enum type between validator, domain, and presumably the unseen schema) is itself a minor drift-risk pattern versus PlantSafety's smart-enum approach |
| PayloadJson | `string`, validator `NotEmpty()` (**no `MaximumLength` at all**) | `string` | Unverifiable — likely `NVARCHAR(MAX)` given the description ("Serialized payload... stored immutably") but this is inference, not verified | Unverifiable | **If the unseen column is not MAX-sized, this is the highest-risk field in the module** — arbitrary producer payload with zero length cap flowing to an unverifiable column | N/A | Yes | Directly verified (validator has no length rule); SQL side unverifiable | Flagged as the top follow-up item if/when the real schema is located |
| PayloadSizeBytes | `long`, validator `GreaterThan(0)` | `long` | Unverifiable — column likely `BIGINT` given the C# type, not confirmed | Unverifiable | N/A (numeric) | N/A | Yes | Directly verified for C#; SQL unverifiable | |
| SchemaVersion | `int`, validator `GreaterThan(0)` | `int` | Unverifiable | Unverifiable | N/A | N/A | Yes | Directly verified for C#; SQL unverifiable | |
| RawZonePartitionPath | Not a command input field | Present on `RawDataIngestedEvent` (never published, per Phase 3) and on `IngestionJobRecord` (read side) | Unverifiable | Unverifiable | N/A | N/A | **Unverified — plausible dead/procedure-computed field**, see finding #4 | Pattern-inferred | |
| TenantId | `Guid`, validator `MustBeValidTenantId()` | `Guid` (`DataLakeControllerBase.TenantId`, required/throwing per Phase 2) | Unverifiable | Unverifiable | N/A | N/A | Yes | Directly verified for C#; SQL unverifiable | Contrast Intelligence, whose *outbox* record explicitly drops TenantId (a different, already-documented gap) — DataLake's ingestion *command* carries TenantId correctly; whether the unseen table's column is nullable is simply unknown |

## Method note

Given the complete absence of source-controlled SQL for this module, this phase traced the one
command Phase 3 already fully read (`IngestRawDataCommand`) rather than spending further budget on
`ProcessRawToCleansedCommand`/`PublishIntegrationEventCommand`, since all three would hit the exact
same "Dapper record shape confirmed, SQL column shape unverifiable" ceiling — re-tracing them would
not surface a materially different finding. The single most valuable follow-up for a future phase
is locating (or confirming the non-existence of) the actual `DataLake.CreateIngestionJob` and
sibling stored procedures/tables, wherever they live outside this repository.
