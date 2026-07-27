# Integration Test Restoration Execution Blueprint

Documentation only — no code, SQL, or UI changes are made by this
document. This is the execution-level follow-on to Step A/C (design),
Triage 1/3 (triage), and the SQL Error-Handling Modernization Plan
(prerequisite sequencing). It assumes those documents' findings as given
and does not re-derive them except where this blueprint found new,
more precise detail.

## 1. `ICultureRepository` Implementation Blueprint

### 1.1 The correct interface contract

**`PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository`**
is the contract to implement (Step A's conclusion, restated as the
authoritative target here):

```csharp
public interface ICultureRepository
{
    Task AddAsync(Culture culture, CancellationToken cancellationToken);
}
```

The Domain-layer `PlantSafety.Domain.Analytics.Culture.Interfaces.ICultureRepository`
(with `GetByIdAsync`/`ListAsync`/`AddAsync`/`UpdateAsync`) remains
confirmed dead code (zero consumers) and is **not** part of this
blueprint's implementation target — its deletion is a separate,
independent cleanup step, not a prerequisite or a dependency of this
work.

### 1.2 The correct persistence shape

Derived directly from `Culture.CreateAsync`'s current (in-progress)
implementation and the `Culture` aggregate's constructor parameters, the
`[Analytics].[Culture]` table needs at minimum these columns:

| Column | Type (inferred from usage) | Notes |
|---|---|---|
| `CultureId` | `UNIQUEIDENTIFIER` | Primary key |
| `TenantId` | `UNIQUEIDENTIFIER` | Tenant-scoping, per this codebase's universal convention |
| `SiteId` | `UNIQUEIDENTIFIER` | |
| `Status` | `NVARCHAR(30)` | Stored as `.ToString()` of a status enum/value object |
| `SurveyPeriod` | `NVARCHAR(20)` | |
| `CultureIndexScore` | `DECIMAL` | |
| `EngagementScore` | `DECIMAL` | |
| `ParticipationRate` | `DECIMAL` | |
| `TopRiskTheme` | `NVARCHAR(200)` | |
| `CreatedAtUtc` | `DATETIME2(3)` | Per this codebase's universal audit-column convention |
| `CreatedBy` | `UNIQUEIDENTIFIER` | |
| A concurrency column | — | **Open decision, not resolved by this blueprint** — see Section 1.4 |

This list is derived from the `AddAsync` implementation's parameter
mapping, not from an actual `[Analytics].[Culture]` table DDL file (none
exists — Step A/Triage 1's core finding). **Whether the table itself
already exists under a different/undiscovered name, or needs to be
created from scratch, is the first fact to establish before writing any
procedure** — this blueprint cannot resolve that without a live database
query, which is outside its documentation-only scope.

### 1.3 How to implement it safely once SQL is stabilized

"Once SQL is stabilized" here means: after the SQL Error-Handling
Modernization Plan's Section 1.4 Phase 1 work reaches `Analytics` (Tier 1
Critical in that plan), or at minimum after a deliberate decision to give
Culture's new procedures the full canonical pattern from day one rather
than inheriting `Analytics`'s current 0% TRY/CATCH/audit-write baseline.
Concretely:

1. Confirm table existence/create it (Section 1.2).
2. Write `usp_Culture_Insert`, `usp_Culture_GetById`, `usp_Culture_GetAll`
   with the full three-layer canonical pattern (TRY/CATCH, AuditLog,
   ErrorLog) from the start — **do not** match `Analytics`'s current 0%
   baseline; Culture is new/unfinished work, not legacy debt, so there's
   no "existing convention" argument for skipping it here the way there
   might be for a large pre-existing module.
3. Decide the concurrency column now, per the SQL Modernization Plan's
   Section 2 direction (`ROWVERSION`, canonical going forward) — Culture
   currently has **no** concurrency column at all (confirmed: 0 of 22
   `Analytics` tables have one), so this is a Section 2.4 "Phase 2" case
   (adding new protection), not a migration of an existing column.
4. Land the interface/DI portion (already substantially complete per
   direct inspection of the in-progress background session's files) as
   its own commit, separate from the SQL addition.
5. Verify via the integration test host (Sections 3/4 below) once
   restored — this is the only way to prove the full chain (interface →
   DI → SQL → real database) works together, not just each piece in
   isolation.

### 1.4 Modules that depend on Culture

Precise dependency mapping (corrects an over-broad assumption a
name-based search would produce):

| Consumer | Actually depends on the broken path? | Basis |
|---|---|---|
| `PlantSafety.UI/Modules/Culture` (`CultureApiClient.cs`, `CultureDashboardPage.razor`, `CultureState.cs`) | **Yes — genuinely blocked** | Directly calls the Culture create/read flow that has zero working stored procedures |
| `PlantSafety.UI/Modules/CultureAnalytics` (`CultureAnalyticsApiClient.cs`) | **No — not blocked** | Confirmed by direct inspection: calls `CultureIndex`, `EngagementScoring`, `TrendModeling`, `Behavior` endpoints — four **sibling** repositories under `Analytics.Culture.*` that were already registered and working before this session's Culture investigation began (they were among the 21 originally-wired legacy repositories, not the missing 23rd) |
| `PlantSafety.Application.Analytics.AnalyticsModule` / `AnalyticsPermissions` | **No functional dependency** | Only defines permission-string constants (`"Analytics.Culture.View"`) — doesn't call the repository |
| `PlantSafety.Api.SafetyCulture.*` (`CultureAssessmentsV1Controller`, `StopWorkEventsV1Controller`) | **No — unrelated module** | `SafetyCulture` is a **different backend module** from `Analytics.Culture`, despite the similar name — confirmed via separate DI registration (`SafetyCultureInfrastructureServiceRegistration.cs`) and separate persistence (`CultureAssessmentDapperRepository.cs`, a distinct class). Do not conflate the two when scoping any Culture-related fix. |

**Practical implication**: the blast radius of the Culture gap is smaller
than a name-based search would suggest — one UI module (`Modules/Culture`
specifically), not the whole "Culture" naming family.

## 2. `LegacyRepositoryRegistration.cs` Correction Blueprint

### 2.1 Modules with broken or incomplete registrations

Per Triage 3's exhaustive, twice-verified static audit (matching all 272
declared `I*Repository` interfaces against every DI registration site,
using a multi-line-aware search after an initial single-line regex
produced several false positives): **exactly one** confirmed case —
`ICultureRepository` in `AnalyticsLegacyRepositoryRegistration.cs`. No
other module's registration file has a confirmed missing entry.

### 2.2 Correct DI wiring

For the one confirmed gap:

```csharp
// In AnalyticsLegacyRepositoryRegistration.AddAnalyticsLegacyRepositories(),
// alongside the existing 22 registrations:
services.AddScoped<Application.Common.Interfaces.Persistence.ICultureRepository, CultureRepository>();
```

This line is already present in the in-progress background session's
uncommitted changes, per direct inspection at the time of writing.
**Correct target file, correct pattern, matching every sibling
registration already in that file** — no structural change needed beyond
landing what's already drafted.

### 2.3 Documentation-drift correction (a related, smaller finding)

All six `*LegacyRepositoryRegistration.cs` files (`Analytics`,
`Compliance` + `...Batch2`, `Operational`, `SafetyEvents`, `Features`)
carry an identical, now-stale comment claiming their call site is "a
required, tracked follow-up for a human maintainer" — confirmed false in
all six cases; every one is already invoked from
`InfrastructureServiceRegistration.AddInfrastructure()`. This isn't a
functional gap, but it's included here because it's a DI-adjacent
documentation-correctness issue in the same file family, and leaving it
uncorrected risks a future maintainer wasting time "fixing" a non-problem.

### 2.4 Safe correction plan, sequenced to run once integration tests execute

1. **Land the one real fix** (Section 2.2) — already drafted, needs
   review and commit.
2. **Correct the six stale comments** (Section 2.3) — independent,
   lower-priority, can be batched separately.
3. **Verify via a live `ValidateOnBuild` run** (Section 4) — this is the
   step that actually proves "no other gap exists" beyond what static
   analysis already found. Static analysis is strong evidence, not
   proof; the live host boot is the actual gate.
4. **If the live boot surfaces an additional, previously-undetected
   gap** (possible — static analysis has known blind spots, e.g. factory
   -delegate registrations `AddScoped<T>(sp => ...)` which this session's
   audit had to specifically re-check for after initial false negatives),
   apply the same fix pattern: locate the existing Dapper implementation,
   confirm the correct interface, add one registration line to the
   relevant `*LegacyRepositoryRegistration.cs` file. This is a
   well-rehearsed, low-risk pattern by this point, not a novel problem
   each time it recurs.

## 3. Embedded Schema Restoration Plan (`TestDatabaseFactory`)

### 3.1 Modules missing embedded schema

44 of 47 backend module folders (94%) — every module except
`SafetyEvents` (Incident only, not the module's other ~9 features),
`RiskManagement`, `AccidentManagement`. Restated from Triage 3 with the
SQL Modernization Plan's risk tiers layered on: **every Tier 1 module
(`Compliance`, `Analytics`, `Operational`) is in this missing-coverage
set**, meaning the modules that most need modernization are also the
ones with zero ability to verify it.

### 3.2 Correct schema extraction process

The three existing methods (`ApplyIncidentManagementSchemaAsync`,
`ApplyRiskManagementSchemaAsync`, `ApplyAccidentManagementSchemaAsync`)
establish the pattern to replicate:

1. **Extract, don't hand-write.** Each existing method's embedded DDL
   was derived from the real `.sql` files under that module's
   `Infrastructure/<Module>/Sql/Tables/` and `Sql/Procedures/` folders —
   confirm this is still true for any new module by diffing the embedded
   copy against the real files before assuming they match (Triage 3
   already flagged this coupling as a future drift risk in its own
   Phase 4).
2. **Scope to what the integration tests actually need**, not the whole
   module. `ApplyIncidentManagementSchemaAsync` only embeds the
   `[SafetyEvents].[Incident]` table and its six procedures — not the
   other ~9 `SafetyEvents` sub-features (`Observation`, `NearMisses`,
   `RootCauseAnalysis`, etc.) which have their own, still-missing,
   schema-application methods of their own if/when tests are written for
   them.
3. **Include cross-database dependencies** the module's procedures
   actually reference — `usp_Incident_Create.sql`'s
   `[PlantSafety.AuditDB]`/`[PlantSafety.ErrorDB]` inserts required
   `ApplySharedAuditLoggingSchemaAsync` to already be applied first; any
   new module's procedures following the same canonical pattern (per the
   SQL Modernization Plan's Section 1) will have the same dependency.
4. **Name the method `Apply<Module>SchemaAsync`**, matching the existing
   convention exactly, and add its call to `InitTestHost.InitializeAsync`
   in the same explicit, ordered list style already used there.

### 3.3 Plan for adding schemas without breaking existing tests

- **Additive only, one module at a time.** Each new `Apply<Module>SchemaAsync`
  method is independent of the existing three — adding one doesn't
  require touching `ApplyIncidentManagementSchemaAsync` or its siblings,
  so there's no risk of regressing already-working coverage as a side
  effect.
- **Order of addition should follow the SQL Modernization Plan's risk
  tiers** (Tier 1 first: `Compliance`, `Analytics`, `Operational`), not
  arbitrary/alphabetical order — restated from that plan's Section 4,
  repeated here because it's the concrete action this blueprint executes
  against.
- **Verify each addition in isolation** before adding the next — run the
  full suite after each new module's schema lands, confirming the
  existing Incident/Risk/Accident tests still pass unchanged, before
  starting the next module's extraction. This catches any accidental
  cross-module schema collision (e.g. two modules both trying to
  `CREATE SCHEMA` for an overlapping name) immediately, one module at a
  time, rather than after several have been batched together.

## 4. Integration Test Host Restoration Plan

### 4.1 Correct startup sequence

As currently implemented in `IntegrationTestWebAppFactory` (the target
state to restore, not a proposed change):

1. `IAsyncLifetime.InitializeAsync()` starts the Testcontainers
   `MsSqlContainer` (`_sqlContainer.StartAsync()`).
2. Schema is applied via `TestDatabaseFactory`'s ordered method calls:
   `ApplySchemaAsync` (generic platform tables) →
   `ApplySharedAuditLoggingSchemaAsync` → per-module schema methods
   (currently Incident, Risk, Accident; expanding per Section 3).
3. `TestDataSeeder.SeedAsync` runs.
4. `ConfigureWebHost` overrides configuration (connection strings) via
   `ConfigureAppConfiguration`, executed by the framework when the first
   test in the collection triggers host construction.
5. The DI container builds with `ValidateOnBuild = true` — this is the
   step that currently fails until Section 2's fix lands, and the step
   that will re-validate the whole container once it does.

**This sequence itself does not need to change** — it's correctly
designed; the restoration work is entirely about fixing what blocks step
5 from succeeding (Section 2) and expanding what step 2 covers (Section
3), not about redesigning the sequence.

### 4.2 How Docker SQL should be initialized automatically

Already automatic via Testcontainers — `new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build()`
provisions a fresh container per test collection run, no manual Docker
commands required **as long as Docker itself is running** on the host
machine. This session's own experience is the relevant caution here:
Docker was not running when earlier verification attempts were made,
which silently prevented any integration test from executing (surfacing
as a `DockerUnavailableException` at container-build time, a clear,
diagnosable error — not a silent skip). **Recommendation for whoever
executes this blueprint**: confirm `docker version` succeeds before
attempting any integration test run; if it's not running, start Docker
Desktop and wait for it to report ready (observed to take roughly 20
seconds in this session) before proceeding — this is an environment
precondition, not something the test host itself can or should automate.

### 4.3 How to validate the test host is fully functional

A staged validation, cheapest/most-diagnostic checks first:

1. `docker version` succeeds.
2. `dotnet build PlantSafety.IntegrationTests/PlantSafety.IntegrationTests.csproj -tl:off` — `0 Error(s)`. (`-tl:off` disables the MSBuild terminal logger, which this session found silently swallows output in this environment — without it, `dotnet build`/`dotnet test` can appear to hang or produce no output at all.)
3. Run **one single, arbitrary, non-Culture, non-newly-added-module**
   test via the native xUnit v3 executable
   (`PlantSafety.IntegrationTests.exe` under `bin/Debug/net8.0/` —
   **not** `dotnet test`, which this session found produces no usable
   console output in this environment) — e.g. an existing
   `IncidentLifecycleIntegrationTests` test. Success here proves the
   shared `WebApplicationFactory` host itself boots (Section 2's fix is
   live), isolated from any newly-added module's own correctness.
4. Only after step 3 succeeds, run the full suite and record: total
   tests, pass count, fail count, and specifically watch for any
   `Collection fixture type '...' threw in InitializeAsync`/`threw in
   its constructor` failure shape — the signature of a container-level
   failure (Section 2's class of problem), which should be triaged
   before any individual test-assertion failure.
5. **Record the resulting baseline** (test count, pass count) — per
   Step C's own observation, no verified baseline currently exists to
   compare future runs against, since this may be the first time the
   full suite has ever actually executed end-to-end.

## 5. Full Integration Suite Bring-Up Plan

### 5.1 Failing or non-executing tests, as currently known

- **All tests, currently**: blocked at container-build time by Section
  2's `ICultureRepository` gap — this is a total, not partial, block
  as of this writing.
- **Once unblocked**, tests fall into three categories based on this
  session's findings:
  - Tests for modules **with** embedded schema (`Incident`, `Risk`,
    `Accident`) — expected to run and, per this session's direct
    verification, pass (723/729-class results were observed for the
    corresponding unit-level and handler-level tests; the equivalent
    live integration run was not completed end-to-end due to the
    Culture block arising mid-session).
  - Tests for modules **without** embedded schema but that don't
    actually touch the database (if any exist) — expected to run and
    pass, unaffected by Section 3's gap.
  - Tests for modules without embedded schema **that do** need a real
    database — expected to fail with a SQL-level error (invalid object
    name, etc.) once the container-level block is lifted, **not** a
    DI-level error — this is the next failure tier to expect and
    triage separately from Section 2's class of problem.

### 5.2 Dependency-aware bring-up sequence

1. **Section 2's fix** (blocking prerequisite for literally everything
   else in this plan).
2. **Section 4.3's validation staging** (steps 1–3) — confirms the fix
   worked before investing further effort.
3. **Full suite run against current (3-module) embedded schema
   coverage** — establishes the honest current baseline (5.1, third
   bullet's expected new-failure-tier included).
4. **Section 3's schema expansion, Tier-1-module-first** — each addition
   re-run against the full suite per Section 3.3's isolation rule, so
   the bring-up sequence is: fix container → validate baseline → add
   `Compliance` schema → validate → add `Analytics` schema → validate →
   add `Operational` schema → validate → continue down the SQL
   Modernization Plan's tier list.
5. **SQL Modernization Plan's own Section 1.4/2.4 rollout** only begins,
   per that plan's own Section 4 sequencing rule, **after** a given
   module's schema is embedded and a baseline test exists for it — this
   blueprint's Section 3/5 work is a hard prerequisite for that plan's
   work, not a parallel, independent track.

## 6. Regression Detection & Safety Gates

### 6.1 How integration tests will detect regressions once restored

- **Snapshot/pinning tests** (the `IncidentSqlPayloadSnapshotTests.cs`
  pattern established this session) — read back the exact persisted row
  shape via raw SQL, bypassing read-side stored procedures that might
  mask a column-level regression. This pattern should be replicated for
  each newly-schema-covered module's write paths, not just relied on for
  Incident.
- **Lifecycle tests** (the `IncidentLifecycleIntegrationTests.cs`
  pattern) — full Create → Update → (Delete/Archive) → Read round trips
  through the real MediatR pipeline, catching behavioral regressions in
  the handler/aggregate/repository chain, not just the raw SQL shape.
- **The container-level `ValidateOnBuild` check itself** — every test run
  re-validates the entire DI graph, so any future registration gap
  (Section 2's class of problem) is caught immediately by the first test
  that runs, not discovered later in production.

### 6.2 How SQL modernization will rely on these tests

Direct dependency, not a loose coupling: the SQL Error-Handling
Modernization Plan's Section 4 already states integration test coverage
is a **hard prerequisite gate**, not a nice-to-have, for exactly the
reason this blueprint exists — a TRY/CATCH addition or a new concurrency
column changes procedure behavior (an operation that previously always
succeeded can now throw or reject), and the only way to confirm that
change didn't also change behavior for the *success* path is a test that
already proves what the success path looks like today. Concurrency
unification specifically (that plan's Section 2.4, Phase 2 — adding
protection to the 202 currently-unprotected tables) is the single
highest-regression-risk piece of work this whole chain leads to, and
should not begin for any module until that module has both embedded
schema (this blueprint's Section 3) and a passing baseline test (this
blueprint's Section 5).

### 6.3 How UI modernization will rely on these tests

Per the UI readiness matrix (Triage 5) and its stated gating criteria:
before any UI write-flow work is trusted for a given module, that
module's backend needs (among other things) proven, non-anemic write
persistence — which integration tests are the mechanism that actually
proves, rather than assumes. Concretely: `PlantSafety.UI/Modules/Culture`
(Section 1.4) should not be considered "UI-ready" until an integration
test exercises its full create/read path against a real database, not
merely until the C# compiles and the DI container builds — the same
standard already applied, and already met, for
`PlantSafety.UI/Modules/IncidentManagement`'s backend this session. Any
future UI modernization work should adopt this as a standing rule: a
module's UI write-flow is not "ready" by this codebase's own emerging
standard until it has the same three things `IncidentManagement` now
has — a non-anemic aggregate, embedded integration test schema, and a
passing lifecycle test proving the full stack.
