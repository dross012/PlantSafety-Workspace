# Phase 6 — Cross-Cutting Findings (referenced from every per-module sql-shape file)

Builds on `command-handler-map/_shared-findings.md` (F7-F13, esp. **F13**'s "zero-implementation"
domain-repository finding for `RegulatoryCalendar`) and `event-map/_shared-findings.md` (F14-F17). This
phase's job was per-transition SQL shape — field-set match, nullability match, enum drift, and
truncation risk — going one level deeper than the Ingestion Discovery's aggregate-level shape audit.
Findings below (G1-G5) consolidate the most consequential results across all 25 `sql-shape/*.md` files
(17 carried forward from before this phase's interruption, 8 produced this phase).

## G1 — The "zero-implementation domain repository" pattern (F13) is not a one-off — it recurs across at least 13 aggregates in 3 different modules
`command-handler-map/_shared-findings.md` F13 established this for `RegulatoryCalendar` alone. This
phase's DI-implementation check (grep of each aggregate's fully-qualified domain-interface namespace
against the whole solution, excluding worktree scratch copies) found the identical failure mode — an
interface + factory pair exists in `PlantSafety.Domain`, but zero classes anywhere implement the
repository interface — for all of:
- **Analytics**: `Culture` (`ICultureRepository`), `ScoreCards` (`IScoreCardsRepository`) — 2 of 4
  aggregates in `analytics.md`. (`Engagement`/`PredictiveAnalytics` do have implementing classes.)
- **Compliance**: `Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`,
  `Procedures`, `RegulatoryCalendar` (F13, inherited), and the `Auditing` marker — all 8 of 8
  aggregates in `compliance.md`.
- **SafetyEvents**: `EmergencyManagement`, `IncidentManagement`, `Observation` — all 3 of 3 aggregates
  in `safetyevents.md`.

That is 13 confirmed zero-implementation aggregates (RegulatoryCalendar plus 12 newly confirmed this
phase), plus 2 more (`AiGovernance`/`AiSafetyIntelligence`) with weaker, pattern-inferred evidence of the
same thing. This is a materially sharper finding than "Create has no confirmed SQL destination" (the
framing used through Phases 1-5): for these 13+ aggregates, the failure isn't "the stored procedure
doesn't exist" — it's "the C# class that would call any stored procedure doesn't exist either." A
`Create` call against any of these 13 would fail at dependency-injection resolution (a missing-service
exception at container build time or first resolution), before the question of SQL shape could even
arise. Two of the 13 (`Culture`, `Procedures`) additionally have a same-named-but-unrelated read-only
class nearby that could cause a shallow audit to wrongly conclude the write path works — see G2.

## G2 — Naming collisions between a real (but unrelated) class and the missing domain repository are a recurring trap, confirmed twice this phase
`analytics.md` (`Culture`) and `compliance.md` (`Procedures`) each have a same-named
`XRepository`/`IXRepository` class elsewhere in the solution that does call real SQL and does have an
implementation — but implements a completely different interface, scoped to a different module's
read-model, not the domain write-interface this phase was checking. `CultureRepository.cs`
(Infrastructure, read-only `GetById`/`GetAll` against real procs) does not implement
`ICultureRepository` (Domain). `ProceduresDapperRepository.cs` (Infrastructure, under
`Auditing/AuditEngagement/Procedures/`) implements a different `IProceduresRepository`
(`PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories` namespace), not
`PlantSafety.Domain.Compliance.Procedures.Interfaces.IProceduresRepository`. A short-name-only grep
(rather than the fully-qualified-namespace grep this phase used) would have wrongly concluded both
aggregates have working write paths. This mirrors `RegulatoryCalendar`'s own trap from F13 (a
same-named `RegulatoryCalendarRepository` class that calls 2 nonexistent procs) — three confirmed
instances of the same trap shape now, all in Create-only/marker-adjacent aggregates.

## G3 — Truncation-risk solution-wide count: TriggerAlertCommand's silent-truncation bug remains the only confirmed instance
This phase specifically checked DataLake's and Intelligence's parameter-binding patterns, since both
modules use a different mechanism (Dapper `DynamicParameters.Add(name, value)` / anonymous-object
binding) than PlantSafety's `StoredProcedureBuilder.AddString(name, value, size)`. Neither module's
pattern carries a fixed client-declared byte size for string parameters — Dapper infers parameter size
from the value's actual length at call time, so there is no equivalent of `TriggerAlertCommand`'s bug (a
value silently truncated to a caller-declared cap before ever reaching SQL Server). The residual risk in
DataLake/Intelligence is the opposite failure mode — an unconfirmed, proc-body-dependent possibility of
a loud SQL-side truncation/overflow error, which is a fail-loud, not fail-silent, risk and was not
confirmed to have occurred anywhere (no proc bodies exist to check column widths against). Combined
with the 17 previously-existing `sql-shape` files' rows (all individually re-checked in prior phases and
found either safely-validated or, in one case, `CreateCorrectiveActionCommand.Description`/
`accidentmanagement.md` #3, an unconfirmed/likely 2000-char gap with no confirmed truncation instance
yet observed) — the solution-wide count of confirmed truncation-risk instances beyond
`TriggerAlertCommand` is zero. Every other flagged item across all 25 files is either a
validator-coverage gap (present but unconfirmed to have fired) or moot because no implementing
repository/stored procedure exists at all to carry out a truncating write in the first place.

## G4 — No new confirmed enum-drift instance; every candidate remains structurally-possible-but-unconfirmed
Every module's SQL-shape file across all 25 files that discusses an enum-shaped field
(`Category`/`Severity`/`RootCause` in AccidentManagement; `Status`/level fields in Risk;
`IngestionPattern` in DataLake, a string allow-list rather than a true enum) reports the same shape of
finding: no CHECK constraint or lookup table exists to verify the C# enum/smart-enum against, because
either (a) no such constraint was found on the live tables that do exist, or (b) no SQL
destination/proc body exists at all to check. Zero instances of an actually-observed enum-value
mismatch (e.g., a persisted row whose stored code value no longer parses against the current C# enum)
were found anywhere in the solution across Phases 1-6. This finding stands unchanged from the 17
pre-existing files; this phase's 8 new files neither add nor refute a confirmed drift instance — the
risk remains entirely structural (possible, unconfirmed) solution-wide.

## G5 — The single worst shape-alignment issue found solution-wide is not a shape mismatch at all — it's the absence of anything to have a shape
Ranked against every other finding in this document set (`TriggerAlertCommand`'s truncation bug;
`ConcurrencyToken`'s dead optimistic-concurrency column; `UpdatedBy` always reflecting `CreatedBy`; the
46/130 best-effort/anonymous AuditLog coverage), the single worst issue is G1: 13+ aggregates across 3
modules (Analytics, Compliance, SafetyEvents) whose domain write-repository interface has zero
registered implementations anywhere in the solution. Every other finding in this phase and its
predecessors describes a real write path that behaves incorrectly in some dimension (wrong actor
attribution, silent truncation, missing old-value in an event, best-effort audit logging); G1 describes
13+ write paths that do not exist as executable code at all — not "misshapen," but structurally absent,
meaning any `Create` call against one of them fails at DI-container resolution before a single line of
shape-validation logic (or, for that matter, any SQL statement) could ever run. This is a strictly more
severe failure category than a mismatched field or an unenforced enum, because it is not recoverable by
fixing a validator or widening a column — it requires writing an entire repository implementation,
wiring DI registration, and (for 12 of the 13) confirming or creating a stored procedure that doesn't
yet exist either. Combined with F13's original framing ("this is one level more broken than the proc
doesn't exist"), G1 generalizes that severity ranking from 1 aggregate to a confirmed 13, making it the
single worst — and most solution-wide — shape-alignment finding of the entire Lifecycle Coverage Audit
to date.

## Practical reading for every per-module file in this phase
Where a module's aggregates were confirmed (this phase or inherited) to have zero implementing
repository class, "Field-Set Match?"/"Nullability Match?"/"Enum Drift?"/"Truncation Risk?" are uniformly
scored N/A rather than "No" or "Unconfirmed" — there is no parameter-binding code, and therefore no
shape, to evaluate. This is a deliberate scoring choice consistent across `analytics.md`,
`compliance.md`, `safetyevents.md`, and most of `emerging-createonly.md`: N/A means "the question does
not apply here," distinct from "Unconfirmed" (the question applies but wasn't checked) used elsewhere
in this document set for rows this phase didn't have time to individually re-open.
