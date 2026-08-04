# Phase 9 §3 — Deployment & Release Architecture: Hardening & Release Readiness Audit

Status: verified against the actual codebase on 2026-08-03 (Documentation Agent, docs-only pass).
Solution root: `D:\PlantSafety\source\PlantSafety`. Grounded in direct file inspection plus
`docs\Phase6\deployment-release-architecture.md` (prior pass) and
`docs\Phase7\versioning-conventions.md` (re-verified, not re-derived from scratch, in item 3
below). Constraint honored: **no new modules, no drift** — every fix below is additive-only
inside the existing `.github\workflows\ci.yml`; no new deploy target, no renamed stage, no
fabricated automation script.

## Summary

| # | Item | Finding |
|---|---|---|
| 1 | CI/CD for all six projects | **Solid, no gap.** `build` job runs `dotnet build` on the whole `.sln`, which includes all six required projects. |
| 2 | Migration sequencing | **Genuine doc gap, closed.** New `docs\Phase9\migration-sequencing.md`. |
| 3 | Artifact versioning consistency | **Solid, re-verified.** API/event/data-contract versioning all consistent through Phase 8 (`GlobalSafetyDashboardV1Controller`, `CrossSiteActionEvent`). |
| 4 | Release notes automation | **Genuine gap, no automation exists.** Concrete minimal recommendation below (not built — out of scope). |
| 5 | Environment promotion stage naming | **Naming mismatch confirmed and flagged, not renamed.** Real stages are Dev/Test/Staging/Prod, not Dev/QA/Staging/Prod. |
| 6 | Health check gating for deployments | **Genuine gap — still fully manual.** Real health checks exist; nothing in the pipeline calls them. |
| 7 | Integration smoke tests in CI | **Genuine gap, fixed directly (additive).** Added `integration-tests` job to `.github\workflows\ci.yml`. |

---

## 1. CI/CD pipelines for all six projects — SOLID, no gap

`.github\workflows\ci.yml`'s `build` job (lines ~37-52) runs:

```
dotnet restore "${{ env.SOLUTION_PATH }}"
dotnet build "${{ env.SOLUTION_PATH }}" --configuration Release --no-restore
```

`SOLUTION_PATH` = `PlantSafety/PlantSafety.sln`. `PlantSafety.sln` (grepped directly) contains
`PlantSafety.Api`, `PlantSafety.Domain`, `PlantSafety.Application`, `PlantSafety.Infrastructure`,
`PlantSafety.SharedKernel`, `PlantSafety.Tests` (plus `PlantSafety.IntegrationTests`,
`PlantSafety.UI`, and the `Shared.*` satellite projects). All six required projects (SharedKernel,
Domain, Application, Infrastructure, Api, Tests) build as part of the whole-solution `dotnet build`
— there is no missing project in the build matrix. No YAML change was needed for this item.

`test` (lines ~57-80) runs `PlantSafety.Tests` specifically. `PlantSafety.IntegrationTests` was
NOT run anywhere — that gap is item 7, fixed below.

## 2. Migration sequencing rules — genuine gap, closed

See **`docs\Phase9\migration-sequencing.md`** (new). Key findings, grounded in the real migration
tree (`PlantSafety.Infrastructure\Persistence\Migrations\Phase1` through `Phase20-ReleaseManagement`,
20 phase folders):

- Every script uses `IF NOT EXISTS (...) BEGIN ... END` for DDL and `CREATE OR ALTER` for
  procedures — confirmed again by reading `Phase19-DeploymentRelease\190-feature-flag-rollout-percentage.sql`
  and `Phase20-ReleaseManagement\200-tenant-user-region-feature-flag-overrides.sql` in full.
- Real cross-migration dependency found: `190-feature-flag-rollout-percentage.sql`'s
  `ALTER TABLE [System].[FeatureFlag] ADD RolloutPercentage` requires that table to already exist,
  which it does via `Phase14-Governance\144-feature-flags.sql` (144 < 190, satisfied by numeric
  ordering). `200-...`'s four new `[Release]` schema tables have no SQL-level FK dependency on
  `190-...` (precedence is resolved in C#'s `IFeatureFlagResolver`, not a foreign key).
- Practical rule for this codebase: schema/table changes a new code path *reads* must land
  pre-deploy (e.g. `FeatureRolloutEvaluator`/`SetFeatureFlagRolloutPercentageCommand` require
  `190-...` to have run first); `CREATE OR ALTER` procedure changes are backward-compatible by
  this repo's convention and may go with-or-after code; net-new, not-yet-consumed tables (like
  `Phase20`'s `[Release].*` tables) are safe either way.
- No down-scripts exist anywhere in the tree; "rollback" in this codebase's real convention is
  redeploy-the-previous-build, not reverse-migrate — consistent with the existing
  `Phase18-ExpressChecklist\DR-manual-restore-runbook.md` philosophy.
- One latent (not yet triggered) ordering risk flagged: `ci.yml`'s `for f in .../Migrations/**/*.sql`
  glob sorts phase **folders** lexicographically, so `Phase2` sorts after `Phase19` — harmless
  today (no low-phase script depends on a higher one), but a precise, scoped YAML follow-up
  (sort by numeric phase prefix instead of shell glob order) is recommended rather than fixed
  silently in this pass, since it changes pipeline execution behavior beyond a pure additive step.

## 3. Artifact versioning (API, contracts, event mesh) — re-verified, consistent

Re-derived directly from the current codebase (not just trusted from the Phase 7 doc, which
itself is dated 2026-08-03 and already covers this):

- `PlantSafety.Api\GlobalSafetyInsights\Controllers\V1\GlobalSafetyDashboardV1Controller.cs`
  (Phase 8): `[ApiVersion("1.0")]` + `[Route("api/v{version:apiVersion}/global-safety-dashboard")]`
  — identical pattern to every other `V1Controller` in the codebase (`TenantV1Controller`,
  `BillingV1Controller`, `OperationsConfigurationV1Controller`, etc.).
- `PlantSafety.Domain\GlobalSafetyInsights\Events\CrossSiteActionEvent.cs` (Phase 8): explicit
  `EventVersion => 1`, `SchemaVersion => "2026.1"`, file-header `SchemaVersion: 2026.1` comment —
  identical convention to `EnterpriseEventEnvelope.SchemaVersion`/`IEventUpgrader` used elsewhere
  on the Enterprise Event Mesh.
- `docs\Phase7\versioning-conventions.md` (already current as of today) independently confirms
  API/Events/Data-Contracts are all consistent, documents the real Stored-Procedure
  migration-numbered-folder convention, and correctly flags UI-component versioning as a genuine,
  still-open gap (recommendation only, no code fabricated) — no changes needed to that document.

**Conclusion: no versioning drift found through Phase 8.** No doc or code change was required for
this item.

## 4. Release notes automation — genuine gap, not built (out of scope for a docs pass)

No release-notes generation mechanism exists anywhere in this repository: `.github\workflows\ci.yml`
has no step that diffs migration folders, parses commit messages, or generates a changelog (grepped
for "release"/"changelog" across the workflow file — only doc-comment references, no automation).
`docs\Phase7\release-notes-phase7.md` is a hand-written, one-off summary, not a generated artifact.

The real git commit convention this repository already uses (from `git log --oneline`) is:

```
77c8a461 Phase 8 SQL/UI alignment pass + fix broken tenant impersonation
9fd7ecf5 Phase 8: Multi-Site Safety Intelligence Network (GlobalSafetyInsights)
b345881f Phase 7: SaaS Productization & Global Multi-Tenant Architecture
88f6867d Phase 6: Enterprise Integration & Ecosystem Expansion
```

i.e. `Phase N[.M][: ]<description>` — a phase-numbered convention, not Conventional Commits
(`feat:`/`fix:`/etc.).

**Concrete, minimal recommendation (not built — building a full automation pipeline is out of
scope for this docs-only pass):** add a manually-triggered `workflow_dispatch` job (or a step run
on tag push) that does:

```
git log --oneline <last-tag>..HEAD | grep -E '^[a-f0-9]+ Phase [0-9]+' 
```

grouped by the leading `Phase N` token (which this repo's commit messages already reliably start
with), piped into a release-notes markdown file uploaded as a build artifact (or committed to
`docs\PhaseN\release-notes-phaseN.md`, matching the existing hand-written convention). This is
scoped precisely to the naming convention already established by this repository's own commit
history, not a generic Conventional-Commits changelog generator that wouldn't match how commits
here are actually written. **Not implemented in this pass** — cataloged for a follow-up
build-capable pass per this session's scope limits, consistent with how Phase 6's Documentation
Agent pass catalogued rather than built C#/YAML gaps it found.

## 5. Environment promotion rules — naming mismatch confirmed, flagged (not renamed)

`.github\workflows\ci.yml`'s real, currently-wired stage names (jobs `deploy-dev`, `deploy-test`,
`deploy-staging`, `deploy-prod`; GitHub Environments `Development`, `Test`, `Staging`,
`Production`) are **Dev / Test / Staging / Prod**, confirmed by direct inspection (lines
239-287 as of this pass, now 239+ after the item-7 addition above).

This Phase 9 task brief's terminology used **Dev / QA / Staging / Prod**. This is a genuine
naming mismatch: "Test" (existing) vs. "QA" (spec'd) are almost certainly intended to refer to
the same environment tier, but they are not the same string, and nothing else in the codebase
(no `QA` environment reference anywhere, grepped) uses "QA" terminology.

**Per this task's own explicit instruction, this was NOT silently renamed** — renaming
`deploy-test`/`environment: Test` to `deploy-qa`/`environment: QA` would require a human to also
rename the corresponding GitHub Environment (with its approval rules/secrets) in the real GitHub
repo settings, which does not exist from this sandbox. Flagged here as an explicit decision point
for whoever owns that GitHub repository configuration.

## 6. Health check gating for deployments — genuine gap, still fully manual

Real health checks that exist and are registered (`PlantSafety.Api\Extensions\ServiceCollectionExtensions.cs`
line ~206 `AddHealthChecks()`, mapped at `/health`, `/health/ready`, `/health/live` in
`PlantSafety.Api\Extensions\ApplicationBuilderExtensions.cs` lines 107/113/120):

- `PlantSafety.Api\HealthChecks\BackupRpoHealthCheck.cs` (`/health/ready`, `backup-rpo` — Phase 6.6
  addition, RPO/RTO comparison against `GetBackupHealthQuery`).
- `PlantSafety.Api\HealthChecks\GeoReplicationHealthCheck.cs` (`/health/ready`).
- `PlantSafety.Api\HealthChecks\ServiceBusHealthCheck.cs` (`/health/ready` — verifies Azure
  Service Bus namespace connectivity via `DefaultAzureCredential`; this is the concurrent
  Reliability-hardening work's MassTransit-bus-adjacent health check referenced by this task
  brief — confirmed present, dated 2026-06-12, i.e. it already existed before this pass).
- `PlantSafety.Api\HealthChecks\SchemaVersionHealthCheck.cs`.

**Confirmed gap:** `.github\workflows\ci.yml` does not call `/health` or `/health/ready` anywhere,
and none of the `deploy-dev`/`deploy-test`/`deploy-staging`/`deploy-prod` jobs gate promotion on
a health response — each "Deploy" step is a placeholder `echo` (see the file's own header note
on the undetermined target deploy platform). This is consistent with, and unchanged since, the
prior Phase 6 finding: since there is no real deployed endpoint URL for any environment yet (the
deploy steps are intentionally placeholders pending a target-platform decision — Kubernetes vs.
Azure App Service vs. other, per `docs\Phase6\deployment-release-architecture.md` §6.2), there is
genuinely nothing for a CI health-gate step to `curl` against today. **No YAML health-gate step
was added in this pass** — adding one now would mean hardcoding a placeholder URL that doesn't
correspond to any real deployment, which would be fabrication, not a fix. This is cataloged as a
real, fixable gap for the same follow-up build-capable pass that resolves the target-deploy-platform
ambiguity (item 5 of the Phase 6 doc's summary table): once a real environment URL exists, the
fix is a one-line `curl -f https://<env-host>/health/ready || exit 1` step inserted before each
`deploy-*` job's next stage.

## 7. Integration smoke tests in CI — genuine gap, fixed directly (additive)

`PlantSafety.IntegrationTests\PlantSafety.IntegrationTests.csproj` references `Testcontainers`
4.12.0 and `Testcontainers.MsSql` 4.12.0 (confirmed via `PackageReference` grep) — a real,
Docker-container-backed integration test suite. It was never invoked anywhere in
`.github\workflows\ci.yml` prior to this pass (only `test` running `PlantSafety.Tests` existed).

**Fixed directly in `.github\workflows\ci.yml`** (additive-only, safe, low-risk): added a new
`integration-tests` job (after the `test` job, `needs: build`) that restores and runs
`PlantSafety.IntegrationTests` via `dotnet test`, uploading a `.trx` result artifact, exactly
mirroring the existing `test` job's shape. Written as `continue-on-error: true` (soft-fail) for
the same reason `contract-gate` and `dependency-vulnerability-scan` are soft-fail: this step has
never actually run in this pipeline before (Testcontainers needs a Docker daemon, confirmed
unavailable in this sandbox by multiple prior agents this session, but expected to be available on
real GitHub-hosted `ubuntu-latest` runners), so there is no existing green baseline to gate PRs on
yet. A human should flip `continue-on-error` off once a first real run is confirmed green against
an actual GitHub Actions runner.

---

## Files touched this pass

- `docs\Phase9\migration-sequencing.md` — new.
- `docs\Phase9\deployment-release-readiness.md` — new (this file).
- `.github\workflows\ci.yml` — added the `integration-tests` job (item 7) and an updated header
  comment (stage list + Phase 9 naming-mismatch note, item 5). No stage was renamed, no new
  deploy target was added, no job's promotion logic (`needs:`/`environment:`) was changed for any
  existing job.

## Explicitly NOT done in this pass (per role/scope limits)

- No health-check CI gate added (item 6) — would require fabricating a placeholder deployment URL.
- No release-notes automation script built (item 4) — cataloged as a precise, minimal
  recommendation only.
- No migration-glob sort-order fix applied (item 2's §4 latent risk) — flagged as a scoped
  follow-up since it changes pipeline execution semantics beyond a purely additive step.
- No environment stage rename (item 5) — flagged, not silently renamed, per explicit instruction.
- No production C# written for any of the above — this pass is docs/config-artifact-only per this
  session's Documentation Agent role, matching the same scope boundary the Phase 6 pass over this
  same codebase already established (see `docs\Phase6\deployment-release-architecture.md`'s own
  "Scope note").
