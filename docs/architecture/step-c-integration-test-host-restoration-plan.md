# Step C — Integration Test Host Restoration Plan

**Status note:** written by inspecting the current, live (partially
in-progress) state of `InitTestHost.cs`, `TestDatabaseFactory.cs`, and the
Step A/B fixes. Does not modify anything.

## 1. What actually broke, and why it wasn't caught earlier

`PlantSafety.Api`'s DI container is built with
`ServiceProviderOptions.ValidateOnBuild = true` (`Program.cs`), which
eagerly validates that every registered service graph can be constructed
— not just the ones a given request happens to touch. Both the real
Kestrel host and `PlantSafety.IntegrationTests`' `WebApplicationFactory<Program>`
build the *same* container, so a missing registration anywhere fails
**both** identically.

The reason this wasn't caught before this session: per this session's own
findings, `PlantSafety.IntegrationTests` had apparently never been
successfully executed end-to-end against a live database up to this
point — every prior confirmation of "IntegrationTests build with 0
errors" in this modernization effort was a *compile* check
(`dotnet build`), not a *run*. `dotnet build` does not invoke
`WebApplicationFactory`, so a `ValidateOnBuild` failure — which only
surfaces when the host actually starts — was invisible to every
build-only verification step taken until Docker was actually started and
a test was actually executed (which happened for the first time during
this session's Step 5/8 work).

## 2. The specific gap and its fix (Step A)

`CreateCultureCommandHandler` depends on
`PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository`,
which had no registered implementation. Step A's plan document covers the
fix in full; `InitTestHost.cs`'s current in-progress state already
reflects it being resolved (its `ConfigureWebHost` override contains a
comment confirming "No test-only DI stubs are required here anymore").

## 3. Is this the *only* gap, or should more be expected?

**Cannot be fully confirmed without running the container** (out of scope
for this planning document). What can be said from static inspection:

- `ValidateOnBuild = true` validates the *entire* registered service
  graph at `BuildServiceProvider()` time, not lazily per-request — so if
  the Culture fix is the only remaining gap, fixing it should make the
  host build cleanly in one shot, not require further whack-a-mole
  iteration.
- However, this exact class of bug — an interface scaffolded with a
  Dapper implementation but never wired into DI — has a **documented
  systemic history** in this codebase. Six separate
  `*LegacyRepositoryRegistration.cs` files exist
  (`AnalyticsLegacyRepositoryRegistration`,
  `ComplianceLegacyRepositoryRegistration` +
  `ComplianceLegacyRepositoriesBatch2`,
  `OperationalLegacyRepositoryRegistration`,
  `SafetyEventsLegacyRepositoryRegistration`,
  `FeaturesLegacyRepositoryRegistration`), each created specifically to
  close out this exact category of gap for its own module. All six are
  confirmed called from
  `InfrastructureServiceRegistration.AddInfrastructure()` (verified by
  direct inspection — lines 415–422 as of this writing), so the *files*
  aren't the risk; the risk is whether any one of them, like Analytics
  did with `ICultureRepository`, still has an individual interface
  missed inside an otherwise-complete registration list. This can't be
  ruled out by reading the files — `ValidateOnBuild` at actual startup is
  the only reliable check, which is exactly why restoring the ability to
  run the test host at all (this plan's subject) matters beyond just
  unblocking Culture-specific tests.
- **Recommendation**: after the Step A fix lands, the very first
  verification action should be a full, unfiltered `WebApplicationFactory`
  boot (i.e. run *any single* integration test, not a Culture-specific
  one) to confirm the container validates end-to-end. If it fails again
  with a *different* missing service, that confirms the whack-a-mole
  pattern and each subsequent gap should be fixed the same way (find the
  existing Dapper implementation, confirm which interface it should
  implement, add one `AddScoped` line to the relevant
  `*LegacyRepositoryRegistration.cs` file) rather than escalated as a
  design problem — the fix pattern itself is already well-established
  and low-risk.

## 4. Missing mocks/fakes — none expected

Unlike a typical integration test suite, this one does not appear to rely
on test-only fakes/mocks for repository interfaces — `InitTestHost.cs`'s
`ConfigureWebHost` only overrides configuration values (connection
strings), not service registrations. Every dependency is expected to
resolve to its **real** Dapper implementation against the real
Testcontainers SQL Server instance. This is a deliberate design choice
already in place (see `IncidentLifecycleIntegrationTests.cs`'s header
comment: "the real DI container... and the real
`[SafetyEvents].[Incident]` table"), not something this restoration needs
to introduce. No new fake/mock infrastructure is anticipated as part of
restoring the host.

## 5. Host wiring checklist

- [ ] `ICultureRepository` (Application-layer) resolves to a concrete,
      working `CultureRepository` implementation (Step A).
- [ ] The `[Analytics].[Culture]` table and its three stored procedures
      (`usp_Culture_Insert`, `usp_Culture_GetById`, `usp_Culture_GetAll`)
      exist and are applied to the Testcontainers schema — either via a
      migration script `TestDatabaseFactory.ApplySchemaAsync` already
      picks up, or via a new
      `TestDatabaseFactory.ApplyAnalyticsCultureSchemaAsync`-style method
      analogous to `ApplyIncidentManagementSchemaAsync`, called from
      `InitTestHost.InitializeAsync`. **This is currently unconfirmed —
      if `ApplySchemaAsync` doesn't already apply every module's schema
      generically (check its implementation), Culture-specific
      integration tests will still fail at the database even after the
      DI fix, with a different error ("invalid object name") rather than
      a DI resolution failure.**
- [ ] Full container boot succeeds for one arbitrary test outside
      Analytics/Culture, confirming no second gap.
- [ ] `RowVersion`→`NVARCHAR` fixes (Step B) are in place for any module
      whose integration tests are about to be run for the first time as
      part of this restoration, since a previously-unexecuted test suite
      may surface this same defect in modules beyond
      Incident/CorrectiveAction/ProcessManagement.

## 6. Deterministic validation checklist (for whoever executes this)

1. Confirm Docker is running (`docker version`); start Docker Desktop if
   not.
2. `dotnet build PlantSafety.IntegrationTests/PlantSafety.IntegrationTests.csproj -tl:off`
   — must show `0 Error(s)`.
3. Run the built executable directly — `PlantSafety.IntegrationTests.exe`
   under `bin/Debug/net8.0/` — **not** `dotnet test`, which was observed
   in this session to produce no usable console output (MSBuild terminal
   logger output buffering issue specific to this environment).
4. Run a single, arbitrary non-Culture test first (e.g. an existing
   `SafetyEvents` test) to confirm the shared `WebApplicationFactory` host
   itself boots cleanly — this isolates "container-wide DI failure" from
   "test-specific failure" before investing time in broader runs.
5. Run the full suite. Record: total tests, pass count, fail count, and —
   critically — whether any failure is a
   `Collection fixture type '...' threw in InitializeAsync`/
   `threw in its constructor` shape (this is the signature of a
   container-level failure, distinct from an individual test assertion
   failure, and should be triaged first).
6. For any SQL-level failure (schema/procedure not found, conversion
   errors), cross-reference against Step B's known-fixed list before
   assuming it's a new defect.
7. Once a clean full-suite run is achieved, that run's numbers become the
   new baseline for future comparison — record it (test count, pass
   count) in this document's revision history or a follow-up commit
   message, since no prior verified baseline exists to compare against.
