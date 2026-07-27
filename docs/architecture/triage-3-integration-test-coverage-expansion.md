# Triage 3 — Integration Test Coverage Expansion Plan

## 1. Modules missing embedded schema — the actual scale of the gap

`PlantSafety.IntegrationTests/Setup/TestDatabaseFactory.cs` provides
exactly **five** schema-application methods:

- `ApplySchemaAsync` — generic platform infrastructure only (`[System].[OutboxMessages]`, etc.), not module-specific.
- `ApplySharedAuditLoggingSchemaAsync` — the shared cross-cutting audit log.
- `ApplyIncidentManagementSchemaAsync`
- `ApplyRiskManagementSchemaAsync`
- `ApplyAccidentManagementSchemaAsync`

Against **~47 real domain module folders** under `PlantSafety.Infrastructure`
(counting top-level directories, excluding infrastructure plumbing like
`DependencyInjection`, `Common`, `Persistence`, `Caching`, `Messaging`,
`Search`, `HealthChecks`, `FileStorage`, `ExternalServices`, `Identity`,
`BackgroundJobs`), only **3** — `SafetyEvents` (partially, via Incident
only), `RiskManagement`, `AccidentManagement` — have any embedded
Testcontainers schema at all. That's roughly **6% module coverage**.

**Concretely, this means**: for the other ~44 module folders (including
`Analytics`, `Compliance`, `Scheduling`, `PPE`, `PermitToWork`,
`Observability`, `ContinuousImprovement`, `ProcessManagement`, and every
other domain module), **no integration test in this project can exercise
a real stored procedure against a real database today** — any test that
exists for those modules either doesn't touch the database layer, or
would fail at "invalid object name" the moment it tried to, since the
schema was never applied to the Testcontainers instance.

This is a materially larger gap than the Culture-specific one this
session focused on — Culture's problem was that its *procedures don't
exist at all*; most of these ~44 other modules' procedures likely do
exist (Step F counted 537 total procedure files across the whole tree),
they're just never applied to a test database, so their correctness is
entirely unverified by this test project regardless of whether the C#
code compiles.

## 2. Why this matters beyond "more tests would be nice"

This session found two real, previously-undetected defects
(`CONVERT(NVARCHAR, RowVersion, ...)` in Step B/earlier work, and
Culture's complete persistence gap in Step A) purely by getting **one**
module's integration tests to actually execute against a live database
for what may be the first time. If the RowVersion conversion bug pattern
or an equally real defect exists in any of the other ~500 procedures this
project has never run against a real SQL Server, it is currently
undiscoverable by this test suite — not because a test exists and passes
incorrectly, but because no test exercises the SQL at all for most
modules.

## 3. Plan for restoring/expanding coverage

**Do not attempt to add embedded schema for all ~44 remaining modules in
one pass** — each addition is a real, module-specific piece of work (the
existing three each span hundreds of lines replicating table + procedure
DDL), and blindly generating 44 more without domain review risks
introducing the same kind of copy-paste-propagated defect (Triage 2) this
session found, just embedded in test infrastructure instead of
production procedures.

### Phase 1 — restore the baseline (blocking everything else)

Fix the `ICultureRepository` gap (Step A/Triage 1) so the shared
`WebApplicationFactory` host can build at all — until then, **zero**
integration tests in this project can run, for any module, embedded
schema or not. This is already in progress.

### Phase 2 — triage which modules most need coverage first

Prioritize by risk, not alphabetically or by convenience:

1. **Modules sharing Mechanism 1's concurrency pattern** (Triage 2) —
   `ContinuousImprovement` (partially covered already via the Step 5/8
   snapshot-test work this session) and `ProcessManagement` — since
   those are confirmed to have had the exact class of bug already found
   once, proving the fix is live (not just statically correct) matters
   most here.
2. **Modules with confirmed anemic write paths** — `Compliance` (per the
   extraction blueprint: only 3 of ~124 actions write, none touch the
   domain layer) — an integration test here would immediately surface
   whether those 3 write actions even work end-to-end, which is currently
   unknown.
3. **Modules with existing UI write-flow pages** — cross-reference
   against the UI readiness matrix (Triage 5) — a module with a live
   `Pages/*Create*.razor` or `*Edit*.razor` component has real user-facing
   risk if its backend has never been integration-tested, versus a module
   that's read-only in the UI today.

### Phase 3 — add embedded schema module-by-module

For each prioritized module, follow the exact pattern already established
by `ApplyIncidentManagementSchemaAsync`/`ApplyRiskManagementSchemaAsync`/
`ApplyAccidentManagementSchemaAsync`: embed the table + procedure DDL,
wire it into `InitTestHost.InitializeAsync`, then write or extend an
integration test analogous to `IncidentLifecycleIntegrationTests.cs` that
exercises Create/Update/Read through the real MediatR pipeline.

### Phase 4 — reduce future drift risk

Once several more modules have embedded schema, consider whether the
embedded-copy-in-C# approach itself is sustainable at scale — 5 modules
in, the embedded DDL is already substantial duplication of the real
`.sql` files. A future, lower-drift-risk alternative (not proposed for
immediate action, flagged for consideration) would be having
`TestDatabaseFactory` apply the *real* `.sql` files directly from disk
rather than maintaining a parallel embedded copy — this would have caught
the Step B `RowVersion` conversion bug even sooner, since the embedded
copy and the real file could not have silently diverged.

## 4. `*LegacyRepositoryRegistration.cs` risk analysis

Six files exist, one per module family: `AnalyticsLegacyRepositoryRegistration`,
`ComplianceLegacyRepositoryRegistration` + `...Batch2`,
`OperationalLegacyRepositoryRegistration`,
`SafetyEventsLegacyRepositoryRegistration`,
`FeaturesLegacyRepositoryRegistration`.

**Confirmed, via direct inspection**: all six are actually invoked from
`InfrastructureServiceRegistration.AddInfrastructure()` (lines 415–422).

**Documentation drift found**: every one of the six files carries the
identical boilerplate comment — *"wiring the call site there is a
required, tracked follow-up for a human maintainer... intentionally not
edited here to avoid concurrent-edit collisions with other agents"* —
which is **stale in all six cases**, since the call sites are in fact
already wired. This appears to be a copy-paste template comment from
whatever process originally created these files, never updated once the
wiring was actually completed by a later pass. **Recommendation**: update
or remove this comment across all six files in a small, purely
documentation-fixing change — leaving it in place risks a future
maintainer wasting time "fixing" a problem that doesn't exist, or worse,
assuming a module's repositories aren't registered when they are.

**Systemic-gap risk assessment**: an exhaustive, twice-verified static
audit (matching every `I*Repository` interface declared in
`PlantSafety.Application` against every DI registration call site in
`PlantSafety.Infrastructure`, using a multi-line-aware search after an
initial single-line regex produced false positives) found **zero**
unregistered repository interfaces beyond the already-known,
already-being-fixed `ICultureRepository` gap. **Culture appears to have
been an isolated incident, not a symptom of a wider systemic pattern** —
this is stronger and more specific evidence than Step C's earlier,
more cautious hedge ("cannot be fully confirmed without running the
container"). It does not replace an actual live `ValidateOnBuild` run
(which would also catch non-repository DI gaps this repository-focused
audit wouldn't see), but it meaningfully narrows the expected remaining
risk.
