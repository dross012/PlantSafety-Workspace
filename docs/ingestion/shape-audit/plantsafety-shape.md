# Shape Audit — PlantSafety

Phase 5 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `inventory.md`, `classification-matrix.md`, `module-flow/plantsafety.md`, `gaps/*.md`
(Phases 1-4). This document traces DTO/Request → Domain Aggregate → Dapper mapping → SQL table/
stored-procedure shape for PlantSafety's representative flows (reusing Phase 3's traced flows:
AccidentManagement, Alerts, plus a newly-added SafetyCulture/CultureAssessment write path, and the
cross-cutting Outbox/ProcessedEvents tables every one of the ~55 aggregate repositories writes
through). PlantSafety is the only module in the solution with source-controlled SQL (772 `.sql`
files, all under each feature's `*.Infrastructure\<Domain>\Sql\{Tables,Procedures}\`), so it is the
only module where full DTO→SQL-column verification is actually possible — this makes it the most
productive module for this phase and the source of the solution's most severe cross-cutting finding.

## Top 5 worst shape issues in PlantSafety (all directly verified)

1. **Solution-wide outbox provenance corruption — every one of ~55 aggregate write-repositories
   discards the real ActorId/CorrelationId and substitutes hardcoded constants.** Every
   `*DapperRepository.AddAsync` method (confirmed by grep: 55 files, e.g.
   `AccidentDapperRepository.cs:124`, `AlertDapperRepository.cs:140`,
   `CultureAssessmentDapperRepository.cs:111`, `ScorecardDapperRepository.cs:116`) calls
   `outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString())`
   — the literal string `"system"` for ActorId and `Guid.Empty.ToString()` for CorrelationId, on
   **every single call, regardless of the real actor/correlation values the domain event itself
   carries** (e.g. `AccidentCreatedEvent`'s own `Actor`/`CorrelationId`/`CausationId` fields, read
   in full in `Accident.cs:182-188`). `[System].[OutboxMessages].ActorId`/`CorrelationId` are
   `NVARCHAR(100) NOT NULL` columns sized to hold real identifiers — they are instead permanently
   populated with `"system"` and an all-zero GUID string for every domain event ever enqueued by
   the platform. This is not a missing column (contrast Intelligence's `OutboxMessage`, which is
   missing the columns entirely) — the columns exist and are NOT NULL, but are fed constants
   instead of the real values sitting one property away on the same object. Any future audit trail,
   trace correlation, or "who did this" reporting built against `OutboxMessages` is unrecoverable
   for every event ever written by this pattern. **Directly verified**: read `OutboxWriter.cs`,
   `AccidentDapperRepository.cs`, `AlertDapperRepository.cs` in full; confirmed identical call
   shape by grep across all 55 other repositories (only `UnitOfWork.cs:41`'s underlying
   `IUnitOfWork.EnqueueEvent` passes through real parameters — nothing calls that path from a
   repository).
2. **Silent-truncation-by-design in the shared `StoredProcedureBuilder.AddString` helper, used by
   every Dapper repository in the module.** `AddString(name, value, size)`
   (`PlantSafety.Infrastructure\Persistence\Dapper\SqlBuilder\StoredProcedureBuilder.cs:48-52`)
   calls `DynamicParameters.Add(name, value, DbType.String, size: size)`. Setting an explicit
   `Size` on a `SqlParameter` smaller than the actual string length causes the underlying
   `Microsoft.Data.SqlClient` driver to **silently truncate the outgoing value to `size` characters
   before it ever reaches SQL Server** — this is a well-documented ADO.NET behavior, distinct from
   (and more dangerous than) a database-side "string would be truncated" error, because no
   exception is thrown at all. Every field written through this helper (hundreds of columns across
   ~55 aggregates) is exposed to this risk wherever the calling command's own FluentValidation
   `MaximumLength` (if one exists at all) is looser than, equal to, or absent relative to the SQL
   column size passed to `AddString`. Confirmed exploitable concretely in finding #3 below.
   **Directly verified** (helper source read in full).
3. **`TriggerAlertCommand` (Alerts) has zero FluentValidation validator at all** — confirmed by
   `find` returning no `*Validator.cs` file anywhere under `PlantSafety.Application\Alerts\`. Its
   `Title` and `LocationZoneName` fields flow straight from the untrusted `FromDto` factory
   (trimmed only, no length check) through `Alert.Trigger` into
   `.AddString("Title", aggregate.Title, 200)` / `.AddString("LocationZoneName",
   aggregate.Location.ZoneName, 200)` against `[Alerts].[Alert].Title NVARCHAR(200)` /
   `LocationZoneName NVARCHAR(200)`. Per finding #2, a caller-supplied title/zone name longer than
   200 characters is **silently truncated in-flight**, not rejected — the row is written with a
   shortened value and the API returns success. Compare to AccidentManagement (finding #4) where a
   validator happens to be stricter than the column and this risk does not materialize — the two
   sibling feature areas got materially different outcomes from the same shared persistence helper
   depending only on whether someone remembered to add a validator. **Directly verified**
   (`TriggerAlertCommand.cs`, `AlertDapperRepository.cs`, `Alerts\Sql\Tables\Alert.sql`, and
   confirmed via `find` that no validator file exists for any of the 8 Alerts commands).
4. **`[AccidentManagement].[Accident].ConcurrencyToken` (`UNIQUEIDENTIFIER NOT NULL DEFAULT
   NEWID()`) is a dead column** — modeled in SQL for optimistic-concurrency purposes (the table's
   own header comment and column naming imply this intent), but the `Accident` domain aggregate
   has no `ConcurrencyToken`/`RowVersion` property at all, `AccidentRow` (the Dapper read-model)
   never selects it back, and neither `usp_Accident_Insert` nor `usp_Accident_Update`'s C# callers
   ever read or check it before writing. `usp_Accident_Update` does regenerate it
   (`ConcurrencyToken = NEWID()`) on every update, but since nothing ever compares the old value
   before overwriting, this provides **zero actual optimistic-concurrency protection** — two
   concurrent updates to the same Accident will both succeed silently, last-write-wins, exactly the
   race condition the column's presence suggests was supposed to be prevented. **Directly
   verified** (full read of `Accident.cs`, `AccidentDapperRepository.cs`, `Accident.sql`,
   `usp_Accident_Update.sql`).
5. **No CHECK constraints on `Severity`/`Category`/`RootCause`/`SourceType`-style code columns
   anywhere sampled except `[Alerts].[Alert].Status`** (which does have
   `CK_Alert_Status CHECK ([Status] IN (...))`, matching the 6 `AlertStatus` enum values exactly —
   the one positive enum-drift control found this phase). Every other smart-enum-backed string
   column examined (`Accident.Severity`, `Accident.Category`, `Accident.RootCause`,
   `Alert.CategoryCode`, `Alert.SeverityCode`, `Alert.SourceType`) has no CHECK constraint at all —
   the C# smart-enum's `.Code` value is the only thing keeping stored strings in sync with what
   `Enum.Parse`/`AccidentSeverity.From`/`AlertCategory.From` can later parse. If a smart-enum code
   is ever renamed or removed without a data migration, existing rows will fail to rehydrate at
   read time (`ArgumentException: Unknown AccidentSeverity code`) with no earlier warning from the
   schema itself. **Directly verified** for the 2 tables read in full; **pattern-inferred** for the
   other ~50 aggregates sharing the same smart-enum-to-`NVARCHAR`-without-CHECK shape (not
   individually re-opened this phase).

---

## Feature area: AccidentManagement

**Traced flow: Report Accident (Create)** — `CreateAccidentCommand` → `Accident.Create` →
`AccidentDapperRepository.AddAsync` → `usp_Accident_Insert` → `[AccidentManagement].[Accident]`.

Files read in full: `PlantSafety.Application\AccidentManagement\Commands\CreateAccidentCommand.cs`,
`PlantSafety.Application\AccidentManagement\Validation\CreateAccidentCommandValidator.cs`,
`PlantSafety.Domain\AccidentManagement\Aggregates\Accident.cs`,
`PlantSafety.Domain\AccidentManagement\Enums\AccidentSeverity.cs`,
`PlantSafety.Infrastructure\AccidentManagement\Persistence\AccidentDapperRepository.cs`,
`PlantSafety.Infrastructure\AccidentManagement\Sql\Tables\Accident.sql`,
`PlantSafety.Infrastructure\AccidentManagement\Sql\Tables\CorrectiveAction.sql`,
`PlantSafety.Infrastructure\AccidentManagement\Sql\Procedures\usp_Accident_Insert.sql`,
`usp_Accident_Update.sql`.

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| AccidentTitle | `string` (`CreateAccidentCommand.AccidentTitle`), validator `NotEmpty().MaximumLength(256)` | `string` (trimmed) | `NVARCHAR(400) NOT NULL` | Yes | **No** — validator (256) is stricter than column (400); the one case in this sample where the safety margin works as intended | N/A (string) | Yes — `usp_Accident_Insert` | Directly verified | Validator being *stricter* than the column is what saves this field, not any design guarantee |
| Location | `string`, validator `NotEmpty().MaximumLength(256)` | `string` (trimmed) | `NVARCHAR(400) NOT NULL` | Yes | No — same margin as above | N/A | Yes | Directly verified | |
| Severity | `string`, validator `Must(AccidentSeverity.IsValid)` | `AccidentSeverity` smart enum (`.Code`) | `NVARCHAR(30) NOT NULL`, **no CHECK constraint** | Yes | No — `.Code` values (`NearMiss`, `FirstAidOnly`, `RecordableInjury`, `LostTimeInjury`, `Fatality`) all ≤16 chars, well under 30 | **Yes, structurally possible** — no DB-level enforcement; app-level `IsValid`/`From` is the only guard, and a future code rename would silently orphan historic rows at read time | Yes | Directly verified | See Top-5 #5 |
| InjuredPersonCount | `int`, validator `GreaterThanOrEqualTo(0)` | `int` (clamped to ≥0 again in `Accident.Create`) | `INT NOT NULL DEFAULT (0)` | Yes | N/A | N/A | Yes | Directly verified | Belt-and-suspenders clamping (validator + aggregate) is a positive pattern, not a gap |
| OccurredAtUtc | `DateTime`, validator `LessThanOrEqualTo(UtcNow.AddMinutes(5))` | `UtcTimestamp` | `DATETIME2(3) NOT NULL` | Yes | N/A | N/A | Yes | Directly verified | |
| Category | `string`, validator `Must(InvestigationPhase.IsValid)` | `InvestigationPhase` smart enum | `NVARCHAR(100) NOT NULL`, no CHECK | Yes | No (ample headroom) | Yes, structurally (same shape as Severity) | Yes | Directly verified | |
| OwnerId | `Guid`, validator `NotEmpty()` | `UserId` | `UNIQUEIDENTIFIER NOT NULL` | Yes | N/A | N/A | Yes | Directly verified | |
| RegulatoryFilingRequired | *(not a command field — computed)* | `bool = Severity.IsOshaRecordable` | `BIT NOT NULL DEFAULT (0)` | Yes | N/A | N/A | Yes, but derived, not client-supplied | Directly verified | |
| SiteSecuredAndTriaged | *(not on create command; set later via `ConfirmSiteSecuredAndTriaged()`)* | `bool`, defaults `false` | `BIT NOT NULL DEFAULT (0)` | Yes | N/A | N/A | Yes, but only via a lifecycle method not reachable from any traced controller action this phase (`AccidentManagementV1Controller`'s 7 actions per Phase 3 do not include a "confirm site secured" endpoint) — **possible dead field, pattern-inferred, needs Phase 4/6 follow-up on the controller's full action list** | Pattern-inferred | Flagged, not confirmed dead — full controller action list not re-enumerated this phase |
| ConcurrencyToken | *(no command field, no domain property)* | **None on `Accident` aggregate** | `UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID())`, regenerated on every update | N/A — domain has no matching property at all | N/A | N/A | **No — genuinely dead as an optimistic-concurrency control** (written and overwritten, never read/compared) | Directly verified | Top-5 #4 |
| UpdatedAtUtc / UpdatedBy | *(no command field)* | **None on `Accident` aggregate** | `UpdatedAtUtc DATETIME2(3) NULL`, `UpdatedBy UNIQUEIDENTIFIER NULL` on the table; `usp_Accident_Update` sets `UpdatedAtUtc = SYSUTCDATETIME()` but its `@UpdatedBy` param is bound in the repository to `aggregate.CreatedBy.Value` (`AccidentDapperRepository.cs:151`) — i.e. "who updated this row" is always populated with "who originally created it," not the real updating actor (the domain has no separate concept) | Populated, but with the wrong value — a smaller-scale instance of the Top-5 #1 pattern (real "who did this" data discarded/replaced) | Directly verified | Column exists, is populated, but semantically lies |
| CorrectiveAction.Description | `string` (command param on `CreateCorrectiveAction`, no dedicated validator found this phase) | `string` | `NVARCHAR(2000) NOT NULL` | Yes | **Unverified** — `AccidentInvestigationCommandValidators.cs` not opened this phase for length rules on this specific field; `AddString("Description", ca.Description, 2000)` will silently truncate at 2000 per Top-5 #2 if unvalidated | N/A | Yes | Pattern-inferred (validator file not opened) | Follow-up candidate |

**Downstream note (carried from Phase 3, relevant to shape):** `AccidentCreatedEvent` and its 6
investigation-lifecycle siblings have no consumer at all, so none of this table's data ever reaches
a second representation (projection/read-model) to cross-check shape against — the SQL row is the
only surviving representation of an Accident anywhere in the system.

---

## Feature area: Alerts (OperationalSafety)

**Traced flow: Trigger Alert** — `TriggerAlertCommand` → `Alert.Trigger` →
`AlertDapperRepository.AddAsync` → `usp_Alert_Trigger` → `[Alerts].[Alert]`.

Files read in full: `PlantSafety.Application\Alerts\Commands\TriggerAlertCommand.cs`,
`PlantSafety.Domain\Alerts\Alert\Aggregates\Alert.cs`,
`PlantSafety.Infrastructure\Alerts\Alert\Persistence\AlertDapperRepository.cs`,
`PlantSafety.Infrastructure\Alerts\Sql\Tables\Alert.sql`.

**Structural note found this phase, not previously documented:** the `Alerts` feature area has
**three parallel, differently-shaped implementations** living side by side in the same solution:
(1) `PlantSafety.Domain.Alerts.Alert.Aggregates.Alert` (the one actually wired to
`AlertsV1Controller`/`TriggerAlertCommand`, traced below), (2) `PlantSafety.Domain.Operational
.Alerts.Aggregates.Alert` (a much larger, richer aggregate with assignment/SLA/metadata/lookup
tables — `AlertAssignment`, `AlertMetadataEntry`, `AlertSeverityLookup`, etc. — described in its own
files as a "canonical reference module"), and (3) the read-only `OperationalSafety.Alerts`
persistence/read-model (`AlertsReadModel.sql`, `AlertsDapperRepository.cs`). These are not
migrations of one shape into another — they are three concurrently-existing, independently-SQL-backed
representations of "an Alert" with different tables (`[Alerts].[Alert]` vs
`[OperationalSafety].[Alerts]` vs whatever backs `Operational.Alerts`), different column names, and
different lifecycle enums. This is a shape-drift finding in its own right: three sibling
"Alert" shapes exist, and only one of them (traced below) has a live, wired ingestion path per
Phase 3. **Flagged unverified/needs follow-up** whether the other two are genuinely dead code or
reachable via a controller/consumer path this phase didn't re-trace — Phase 3 only confirmed the
live wiring for `PlantSafety.Domain.Alerts.Alert.Aggregates.Alert`.

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Title | `string`, **no validator exists** (Top-5 #3) | `string` (trimmed, non-empty enforced only by an `ArgumentException` inside `Alert.Trigger`, not FluentValidation) | `NVARCHAR(200) NOT NULL` | Yes | **Yes — live silent-truncation risk**, no length cap before `AddString(..., 200)` | N/A | Yes | Directly verified | Top-5 #3 |
| LocationZoneName | `string`, no validator | `AlertLocation.ZoneName` | `NVARCHAR(200) NOT NULL DEFAULT ('Unspecified')` | Yes | **Yes**, same as Title | N/A | Yes | Directly verified | |
| CategoryCode | `string`, no validator (category validity is not `Must()`-checked before hitting the aggregate) | `AlertCategory` smart enum | `NVARCHAR(50) NOT NULL`, no CHECK | Yes | Low (headroom to 50) but **unvalidated category codes are only caught by an aggregate-level exception if `AlertCategory.From`/equivalent throws — not confirmed this phase whether an invalid code is rejected before or after the DB write attempt** | Yes, structurally (no CHECK) | Yes | Directly verified (validator absence), pattern-inferred (exact failure point) | |
| SeverityCode | `string?` (nullable — falls back to category default) | `AlertSeverity` smart enum | `NVARCHAR(20) NOT NULL` | **Command allows null; column is NOT NULL** — reconciled only because `Alert.Trigger` always resolves a non-null `Severity` (`severity ?? effectiveCategory.DefaultSeverity`) before the row is ever built. Nullability mismatch exists at the contract layer but is masked by domain logic, not by a schema-level default | No (headroom, 20 chars vs short codes) | Yes, structurally (no CHECK) | Yes | Directly verified | A caller who never worries about SQL would reasonably assume `string? SeverityCode` on the command means the column can be null too — it can't; the domain quietly fixes this up |
| Status | *(computed, always `"Triggered"` on create)* | `AlertStatus` enum | `NVARCHAR(20) NOT NULL`, **`CK_Alert_Status CHECK (Status IN ('Triggered','Acknowledged','Dispatched','Escalated','Resolved','Closed'))`** | Yes | N/A | **No drift** — this is the one column in the whole sample with a real, matching CHECK constraint | Yes | Directly verified | Positive control, called out in Top-5 #5 |
| CloseOutVerification | `string` (command param on `Close()`, no dedicated validator located this phase) | `string`, non-null-enforced via `ArgumentException` | `NVARCHAR(4000) NULL` | **Domain requires non-empty (throws if whitespace); column allows NULL** — a mismatch in the conservative direction (DB is more permissive than the domain), not a data-loss risk, but notable: the column's own nullability doesn't reflect a real invariant the aggregate enforces | Unverified (4000 is generous) | N/A | Yes, via `Close()` | Directly verified (aggregate rule), pattern-inferred (no validator file located) | |
| RowVersion | N/A | **No matching domain property** | `ROWVERSION` (SQL Server auto-maintained) | N/A | N/A | N/A | **Yes, automatically, by SQL Server itself — but never read back into `AlertRow`/the aggregate**, so — like AccidentManagement's `ConcurrencyToken` — this optimistic-concurrency mechanism exists in the schema and is entirely unused by the application | Directly verified | Second instance of the same pattern as Top-5 #4 |
| ActorId (on delivery/response-action child writes) | `PerformedByActor actorId` parameter, threaded through domain methods | `AlertResponseActionEntry.TakenBy` (`ActorKind` + optional `UserId`) | `AlertResponseAction.TakenByActorKind NVARCHAR(20) NOT NULL`, `TakenByUserId UNIQUEIDENTIFIER NULL` | Yes | N/A | Unverified whether `ActorKind` enum values are validated against any CHECK (none found on this table) | Yes | Directly verified (table DDL); pattern-inferred (ActorKind enum not opened this phase) | |

---

## Feature area: SafetyCulture (CultureAssessment) — the one genuine Culture *write* path

Distinct from the read-only `Analytics.Culture.Behavior` endpoint Phase 3 traced (which queries
projection tables the stub `CultureCreatedConsumer` never populates). `SafetyCulture
.CultureAssessment` is a separate, independently-wired write path with its own table
(`[SafetyCulture].[CultureAssessment]`) and its own outbox-enqueue call — added to this audit
specifically because it is the one place in the "Culture" name-space that actually persists new
data end-to-end.

Files read in full: `PlantSafety.Infrastructure\SafetyCulture\Sql\Tables\CultureAssessment.sql`,
`PlantSafety.Infrastructure\SafetyCulture\Sql\Procedures\usp_CultureAssessment_Create.sql`.

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| AssessmentTitle | Unverified this phase (command/validator not opened) | Unverified | `NVARCHAR(200) NOT NULL` | Unverified | **Flagged for follow-up** — given the Alerts precedent (Top-5 #3), any `AssessmentTitle` command field without a matching `MaximumLength(≤200)` validator carries the same silent-truncation risk via the shared `AddString` helper | N/A | Yes (insert proc exists and is wired per Phase 1's SafetyCulture controller list) | Pattern-inferred (validator not opened this phase) | |
| Status | Unverified command shape | Unverified | `NVARCHAR(20) NOT NULL DEFAULT ('Created')`, **no CHECK constraint found** | Unverified | N/A | **Yes, structurally** — no CHECK, same gap as AccidentManagement/most-of-Alerts | Yes | Pattern-inferred | |
| OverallCultureScore / LeadershipEngagementScore | Unverified | Unverified | `DECIMAL(5,2) NULL` | Unverified | N/A (numeric, not string) | N/A | Unverified — insert procedure accepts them as optional (`= NULL`) parameters; whether any command handler ever supplies a non-null value was not traced this phase | Pattern-inferred | `DECIMAL(5,2)` caps the value at 999.99 — if this is meant to be a 0-100 score this is generous headroom, not a risk, but the actual valid range is not enforced by a CHECK either |
| Summary | Unverified | Unverified | `NVARCHAR(2000) NOT NULL DEFAULT ('')` | Unverified | Same class of risk as AssessmentTitle | N/A | Yes | Pattern-inferred | |
| RelatedRiskManagementHazardId | Unverified | Unverified | `UNIQUEIDENTIFIER NULL`, **no FK constraint to a RiskManagement table found in this file** | Unverified | N/A | N/A | Unverified whether any command ever populates this — **candidate dead/optional cross-reference field**, not confirmed either way this phase | Pattern-inferred | Follow-up candidate: a nullable cross-module reference with no referential integrity enforcement is worth a dedicated look in Phase 6 (cross-module integration) |

---

## Cross-cutting: `[System].[OutboxMessages]` / `[System].[ProcessedEvents]`

Every one of the ~55 aggregate write paths in PlantSafety funnels through these two tables. Traced
directly: `OutboxWriter.cs` (full read), `OutboxMessages.sql`, `ProcessedEvents.sql`,
`IdempotentConsumerBase.cs` (Phase 2).

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| TenantId | `Guid` on every aggregate | `Guid`, stringified via `.ToString()` before enqueue | `NVARCHAR(100) NOT NULL` | Yes (a GUID string always fits in 100 chars) | No | N/A | Yes, correctly | Directly verified | The one field in this table populated with real data |
| ActorId | Real `PerformedByActor`/`UserId` exists on every domain event | Discarded — replaced with literal `"system"` at every call site | `NVARCHAR(100) NOT NULL` | N/A (constant) | N/A | N/A | **No — populated with a constant, not the real value** | Directly verified | Top-5 #1 |
| CorrelationId | Real `CorrelationId`/`CausationId` exist on every domain event | Discarded — replaced with `Guid.Empty.ToString()` at every call site | `NVARCHAR(100) NOT NULL` | N/A (constant) | N/A | N/A | **No — same as ActorId** | Directly verified | Top-5 #1 |
| Payload | The full domain event object | Serialized via `JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), ...)` | `NVARCHAR(MAX) NOT NULL` | Yes | No (MAX) | N/A | Yes — this is the only place the real Actor/CorrelationId survive, buried inside the JSON blob, invisible to any query/monitoring against the outbox table's own columns | Directly verified | Mirrors the exact concern Phase 4 raised for Intelligence's outbox (opaque payload vs first-class columns) — PlantSafety's case is arguably worse because the first-class columns *exist* and are simply fed wrong data, not merely absent |
| EventId (ProcessedEvents) | N/A (idempotency ledger only) | N/A | `UNIQUEIDENTIFIER NOT NULL PRIMARY KEY` | Yes | N/A | N/A | Yes, by the 22 `*CreatedConsumer` stubs (once/if actually invoked — see Phase 3/4, currently unregistered with MassTransit) | Directly verified | Minimal, correctly-shaped table; no issues found |

## Method note

Given ~30+ PlantSafety feature areas and ~55 aggregate write-repositories, this phase traced 3 full
DTO→SQL chains in depth (AccidentManagement, Alerts, SafetyCulture/CultureAssessment) plus the
cross-cutting Outbox/ProcessedEvents tables every repository shares, and confirmed by grep that the
Outbox provenance-corruption pattern (Top-5 #1) and the `AddString`-truncation-helper pattern
(Top-5 #2) are used identically across all ~55 repositories — these two are treated as
**directly-verified, solution-generalizing** findings, not merely pattern-inferred, because the
grep confirmed byte-for-byte identical call sites rather than "structurally similar" ones. Per-field
nullability/enum/truncation conclusions for the ~50 aggregates not individually opened this phase
(RiskManagement, Checklists, PPEManagement, Schedules, EmergencyManagement, IncidentManagement,
Observation, CompetencyManagement, Compliance sub-areas, ContractorSafety, EmployeeSafety,
MachineSafety, PermitToWork, Scheduling, Dashboards, Scorecards, SafetyPolicies, SafetyProcedures,
etc.) are **not asserted** — they were not opened this phase and should not be assumed to match the
3 traced samples beyond the two confirmed cross-cutting patterns above.
