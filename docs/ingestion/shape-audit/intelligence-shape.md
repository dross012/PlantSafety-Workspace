# Shape Audit — Intelligence

Phase 5 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/intelligence.md`, `classification-matrix.md`, `gaps/*.md` (Phases 1-4).

## Top findings for Intelligence

1. **Same foundational gap as DataLake: zero `.sql` files exist anywhere in the Intelligence
   module** (`find Intelligence -iname "*.sql"` → 0 results, confirmed this phase). The stored
   procedure invoked by `RunInvestigationCommandHandler` (`intelligence.usp_Investigation_Create`,
   per Phase 3) and its siblings (`Sp.Cancel`, `Sp.AddHypothesis`, etc.) have **no corresponding
   script tracked in this repository**. As with DataLake, this phase can verify the DTO → domain
   aggregate → Dapper-record shape, but **cannot verify the actual SQL column types, nullability, or
   constraints** for any Intelligence table. Every conclusion below is capped at "C# shape
   confirmed, SQL shape unverifiable."
2. **`Investigation.RequestedByActorId`/`AssignedReviewerActorId`/all the various `*ActorId`
   parameters threaded through the aggregate are plain `string`, not a `UserId`/`Guid`-typed value
   object** — unlike PlantSafety's consistent use of a strong `UserId` type, or DataLake's
   `RequestingUserId` as a required `Guid`. `RunInvestigationCommand.RequestingUserId` **is**
   strongly typed (`UserId`), but by the time it reaches `Investigation.Open(...,
   requestedByActorId: string, ...)` it has been converted to a raw string somewhere in
   `RunInvestigationCommandHandler` (not re-opened line-by-line this phase, but the aggregate's own
   constructor signature — read in full — only accepts `string`). This is a **type-narrowing shape
   drift** between the validated, strongly-typed command boundary and the domain aggregate itself —
   the aggregate has strictly less type safety than its own inbound command, which is the reverse of
   the direction you'd want type safety to narrow in. Any future code path that calls
   `Investigation.Open` directly (bypassing the command/validator) gets no compile-time or
   FluentValidation guarantee that `requestedByActorId` is even a well-formed GUID string — only the
   aggregate's own `ArgumentException` for null/whitespace guards it, which is a materially weaker
   check than `RunInvestigationCommandValidator`'s `NotEmpty()` on the strongly-typed
   `RequestingUserId.Value`.
3. **`Investigation.Cancel(string cancellationReason, ...)`'s own XML doc comment says "Sanitized
   reason for cancellation. Must not contain PII or stack traces"** — but no sanitization logic is
   present in the method body itself (only a `NotNullOrWhiteSpace` check), and
   `CancelInvestigationCommandValidator` was not opened this phase to confirm whether sanitization
   happens upstream. If it does not happen anywhere in the traced chain, this is a **documented
   invariant with no code enforcing it** — the same "policy exists only as a comment" pattern found
   in Phase 4's authorization-policy gap (`AddPlantSafetyPolicies()` never called), just localized
   to a single field here. Flagged **unverified/needs follow-up**: the validator file was not opened
   this phase.
4. **`RunInvestigationCommandValidator.Subject` caps at `MaximumLength(500)`** — a real, present
   validator (a positive contrast to PlantSafety's Alerts gap) — but since the underlying SQL column
   size is unverifiable (finding #1), whether 500 is a safe cap or itself already too generous (or
   too strict) relative to the real column cannot be confirmed either way.

## Traced flow: Run Investigation

Files read in full: `Intelligence.Application\AIInvestigations\Commands\RunInvestigationCommand.cs`,
`Intelligence.Application\AIInvestigations\Validators\RunInvestigationCommandValidator.cs`,
`Intelligence.Domain\AIInvestigations\Aggregates\Investigation.cs`.

| Field | DTO/Request Type | Domain Type | EF/SQL Column Type | Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Subject | `string`, validator `NotEmpty().MaximumLength(500)` | `InvestigationSubject` value object (`.Value`) | Unverifiable — no `.sql` for this table exists in source | Unverifiable | Unverifiable | N/A | Yes | Directly verified for C#; SQL unverifiable | Finding #1 |
| RequestingUserId | `UserId` (strongly typed), validator `NotEmpty()` on `.Value` | Passed into `Investigation.Open` as **plain `string requestedByActorId`** — type narrows from `UserId` to `string` between command and aggregate | Unverifiable | Unverifiable | N/A | N/A | Yes | Directly verified (both files read in full) | Finding #2 — the type-safety narrowing is the notable shape issue here, independent of the SQL-verification gap |
| Status | Not a command input (computed: always `Open` on creation) | `InvestigationStatus` (a domain type with `.IsTerminal`; not confirmed this phase whether it's a real C# enum or another smart-enum-style class) | Unverifiable | Unverifiable | N/A | Unverifiable whether stored as int/string, and whether any CHECK exists — cannot confirm without the missing DDL | Yes | Directly verified for the state-machine logic; SQL representation unverifiable | |
| OpenedAtUtc / ClosedAtUtc | Not command inputs; both are `DateTime` set via `DateTime.UtcNow` inside the aggregate, **not** `DateTimeOffset` and **not** a `UtcTimestamp` value-object type (contrast PlantSafety's consistent `UtcTimestamp` wrapper, and DataLake's `DateTimeOffset` command fields) | `DateTime` | Unverifiable | Unverifiable | N/A | N/A | Yes | Directly verified | Minor solution-wide inconsistency: three different modules (PlantSafety: `UtcTimestamp`, DataLake: `DateTimeOffset`, Intelligence: bare `DateTime`) use three different UTC-timestamp representations for conceptually the same "when did this happen" data — not itself provably a bug, but a generalizing shape-consistency observation worth flagging for a future normalization pass |
| CancellationReason | `string` (on `Cancel`, command-level validator not opened this phase) | `string`, trimmed, non-empty enforced by `ArgumentException` | Unverifiable | Unverifiable | Unverifiable — doc comment claims "sanitized," no sanitization code found in the traced method body | N/A | Yes, via `Cancel()` | Directly verified (aggregate); validator not opened | Finding #3 |
| FindingCount / MaxFindingsPerInvestigation | Not a DTO field — computed (`_findings.Count`) against `IntelligenceConstants.MaxFindingsPerInvestigation` | `int`, enforced via `IntelligenceDomainException` when exceeded | Unverifiable whether the persisted `Investigation`/`Finding` rows have any matching CHECK/trigger enforcing the same cap at the SQL layer — likely not, given no DDL exists to hold one | Unverifiable | N/A | N/A | Yes, indirectly (each finding add is persisted; the *count* itself does not appear to be a stored column) | Directly verified (aggregate logic) | If the real schema allows unbounded finding rows per investigation, only the C# aggregate stands between "in-memory rehydration" and an inconsistent count if two concurrent requests both pass the in-memory check before either commits — a TOCTOU-style race, not confirmed exploitable this phase, flagged for Phase 7 |

## Cross-reference to Phase 4's Intelligence Outbox finding

Phase 4 (`gaps/orphaned-ingress.md`, `gaps/missing-events.md`) already established that
Intelligence's `OutboxMessage` has no `TenantId`/`ActorId` columns at all — a shape gap this phase
does not need to re-derive. Combined with finding #2 above (`Investigation`'s own actor field
narrowing from `UserId` to bare `string`), the picture for Intelligence is that actor/tenant
identity loses type fidelity at **two separate points** in the same module's pipeline: once between
the command and the aggregate (this phase's finding), and again between the aggregate/event and the
outbox record (Phase 4's finding). Neither is the same bug, but they compound: by the time an
Intelligence domain event would reach a hypothetical downstream consumer, the actor identity has
already been stripped of its type twice and, per Phase 4, may not be present as a queryable column
at all.

## Method note

Given the same missing-SQL ceiling as DataLake, this phase traced only the one flow
(`RunInvestigationCommand`) in full rather than re-tracing `IngestDataCommand`/`TransformDataCommand`
(Intelligence's own `DataLake` subsystem, already traced by Phase 3 for event-flow purposes) — the
DTO/domain shape questions for those two would hit the identical "no SQL to compare against"
ceiling. The type-narrowing finding (#2) is the one Intelligence-specific shape issue distinct from
the SQL-unavailability finding shared with DataLake, and is the highest-value item from this
module for Phase 6/7 follow-up.
