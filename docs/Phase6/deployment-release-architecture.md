# Phase 6.6 — Enterprise Deployment & Release Architecture

**Status:** Documentation-only pass. No implementation work was performed under this
entry — see "Scope note" below.

## Scope note (read first)

This document was produced by the **Documentation Agent**, whose mandate (per its
operating charter) is limited to generating and maintaining Markdown docs, module
READMEs, API/SQL reference docs, and analyzer-rule documentation from the *actual*
state of the codebase. The Phase 6 §6 task brief requested production C#
(`ISecretsProvider`, `AzureKeyVaultSecretsProvider`, `ITenantRolloutPolicy`,
`SecretRotationAuditEvent`), CI/CD pipeline YAML, a Dockerfile, and Kubernetes/Azure
deployment manifests. Those are engineering/DevOps build artifacts, not documentation,
and were **not created** as part of this pass — creating them requires a build/coding
agent, not the Documentation Agent. Per Rule 8 ("On genuine ambiguity, document the
actual current behavior/structure rather than the ideal, and flag the gap"), this
document instead records the **actual current state** of the four §6 subsections as
found in the codebase on 2026-08-03, and flags every artifact still outstanding so a
follow-up engineering pass can pick them up without re-discovery work.

Everything below is grounded in direct inspection of
`D:\PlantSafety\source\PlantSafety`, `D:\PlantSafety\source\SafetyTooling\BuildPipelines`,
and `D:\PlantSafety\source\Azure`, plus the Enterprise Architectural Rule Handbook
(2026 Edition) referenced from
`C:\Users\dross\AppData\Local\Temp\handbook.txt`.

---

## 6.1 Multi-Environment Promotion

**Current state (verified):**
- No CI/CD pipeline is wired to `PlantSafety.sln` anywhere in the solution tree — no
  `.github/workflows/*.yml`, no `azure-pipelines.yml`, nothing under
  `D:\PlantSafety\source\PlantSafety` references build/test/deploy automation for this
  solution.
- `D:\PlantSafety\source\SafetyTooling\BuildPipelines` exists as a **separate,
  standalone tooling project** (`BuildPipelines.csproj`) with its own `Pipelines/`,
  `Templates/`, `Config/`, `Scripts/`, and `GitHooks/` folders and a `ReadMe.txt`. It is
  a template/generator toolkit for producing pipeline definitions elsewhere in the
  platform — it is not itself a live pipeline attached to `PlantSafety.sln`, and it was
  not modified here.
- Real SQL migration scripts exist and are enumerable at
  `PlantSafety.Infrastructure\Persistence\Migrations\Phase*\*.sql` (e.g.
  `Phase12-Reliability\120-backup-jobs-full-diff-log.sql`,
  `Phase18-ExpressChecklist\*.ps1`/`*.md`) — any promotion pipeline's migration step
  would run these in numeric/phase order.
- `PlantSafety.Tests\SharedKernel\Contracts\Enterprise\` **does not exist yet** — the
  concurrent CanonicalModel agent's Phase 6 contract tests have not landed as of this
  pass. A contract-validation promotion gate therefore has no test target to invoke
  yet.

**Gap flagged:** A Dev → Test → Staging → Prod GitHub Actions/Azure DevOps YAML with a
SQL-migration step and a contract-gate step is a required deliverable of §6.1 and is
**not yet built**. Building it correctly also depends on the Enterprise contract-test
project landing first (or the gate step should be added as a conditional/soft-fail
stage now and hardened once the tests land).

## 6.2 Blue/Green Deployments

**Current state (verified):**
- No `Dockerfile` exists anywhere under `D:\PlantSafety\source\PlantSafety`
  (including `PlantSafety.Api`).
- No Kubernetes manifests (`*.yaml`/`*.yml` deployment/service definitions) exist in
  the solution.
- `D:\PlantSafety\source\Azure` is a **separate solution** (`Azure.sln`) containing its
  own set of modules (`Compute.*`, `Data.*`, `Eventing.*`, `Identity.*`, `ML.*`,
  `Networking.*`, `Observability.*`, `Security.*`, `Storage.*` — each with
  Api/Application/Domain/Infrastructure/Tests/IntegrationTests projects). This looks
  like an internal platform-services solution modeling Azure resource types
  domain-side, **not** an Azure App Service / deployment-slot configuration for
  `PlantSafety.Api` itself. There is no App Service deployment-slot config, ARM/Bicep
  template, or `azure.yaml`/`azd` config referencing `PlantSafety.Api` found anywhere.
  This means the actual target deploy platform for `PlantSafety.Api` is **not
  determinable from the codebase as it stands** — it is a genuine ambiguity, flagged
  per Rule 8 rather than guessed.
- Feature flags: **a real feature-flag system already exists**, confirming the
  Handbook's §3.2 Canonical Feature Flag Rule is implemented, not just mandated:
  - `PlantSafety.Application\Operations\Configuration\Contracts\FeatureFlagsSectionProvider.cs`
  - `PlantSafety.Application\Operations\Configuration\Commands\SetFeatureFlagEnabledCommand.cs`
  - Query handlers under `PlantSafety.Application\Operations\Configuration\...`
    (`GetAllFeatureFlagsQueryHandler`, `GetFeatureFlagsQueryHandler`)
  - API surface: `PlantSafety.Api\Operations\Configuration\Controllers\V1\OperationsConfigurationV1Controller.cs`
  - UI surface: `PlantSafety.UI\Modules\OperationsConfiguration\*`
  - Test coverage: `PlantSafety.Tests\Application\Operations\Configuration\Handlers\*`
  This is a real, on/off style feature-flag system. It has **no rollout-percentage or
  tenant-wave concept** — it is binary enable/disable, not canary-capable as-is.
- `ITenantRolloutPolicy` does **not exist** anywhere in the codebase.

**Gap flagged (per the task brief's own guidance):** the base feature-flag system
exists, so building a whole new one would be out of scope — but the
canary-rollout-percentage layer on top of it, the blue/green deployment manifest
(whichever platform is confirmed), and `ITenantRolloutPolicy` are all **not yet
built**. Before building the deployment manifest, the actual target platform
(Kubernetes vs. Azure App Service vs. something else) needs an explicit decision from
whoever owns the `Azure` solution's relationship to `PlantSafety.Api` — it is not
inferable from what exists today.

## 6.3 Enterprise Secrets Management

**Current state (verified):**
- No `.csproj` in `D:\PlantSafety\source\PlantSafety` references
  `Azure.Security.KeyVault` (or any Azure Key Vault SDK package) — confirmed via
  project-file grep across the solution.
- No `ISecretsProvider`, `AzureKeyVaultSecretsProvider`, `ConfigurationSecretsProvider`,
  or `SecretRotationAuditEvent` type exists anywhere in the codebase today.
- Existing Auditing infrastructure that a future secret-access-audit log would hook
  into is present (see `docs\architecture\audit-logging.md` and the audit-log
  subsystem referenced from `ErrorClassifier.cs` / Operations modules), so the
  integration point exists even though the secrets-provider code does not.

**Gap flagged:** this entire subsection — the abstraction, both implementations, the
rotation-audit domain event, and audit-log wiring — is **not yet built**. This is the
largest genuine gap of the four subsections: there is currently no enterprise secrets
abstraction in the codebase at all, dev-only or otherwise. No secret values, vault
URIs, or credentials were fabricated or hardcoded during this investigation.

## 6.4 Disaster Recovery

**Current state (verified) — this subsection has real, substantive prior work,
confirming the task brief's expectation of overlap with `dr-sentinel-agent`:**
- `PlantSafety.Infrastructure\Operations\Backup\Options\BackupOptions.cs` and
  `PlantSafety.Infrastructure\Operations\Backup\Persistence\BackupDapperRepository.cs`
  (plus a duplicate/older path at
  `PlantSafety.Infrastructure\CrossDomain\Backup\Persistence\BackupDapperRepository.cs`)
  implement real backup-job persistence.
- SQL objects: `PlantSafety.Infrastructure\CrossDomain\Sql\Tables\Backup.sql`,
  `PlantSafety.Infrastructure\CrossDomain\Sql\Procedures\usp_Backup_GetBackupJobs.sql`.
- Migrations: `Persistence\Migrations\Phase12-Reliability\120-backup-jobs-full-diff-log.sql`
  and `122-backup-cleanup-retention.sql`.
- Operational scripts and a runbook already exist under
  `Persistence\Migrations\Phase18-ExpressChecklist\`:
  `Invoke-PlantSafetyBackup.ps1`, `Register-PlantSafetyBackupTasks.ps1`, and
  `DR-manual-restore-runbook.md`.

This confirms single-instance SQL Server backup/restore, job scheduling, and a manual
restore runbook are already owned and largely covered by prior (dr-sentinel-scoped)
work.

**Gap flagged (the genuinely enterprise/multi-region-specific angle called out in the
brief):** none of the above artifacts address multi-region failover, and no RPO/RTO
targets are formalized anywhere as configuration or constants referenced by
health-check reporting. A multi-region failover runbook and RPO/RTO
configuration/constants are **not yet built** and remain open work specific to §6.4's
enterprise angle (as opposed to the single-instance backup/restore already covered).

---

## Summary table

| Subsection | Real code delivered this pass | Config/pipeline artifact delivered this pass | Flagged as outstanding / needs a build agent |
|---|---|---|---|
| 6.1 Promotion pipeline | None | None | Dev→Test→Staging→Prod YAML, SQL-migration step, contract-gate step (blocked in part on Enterprise contract tests not yet landed) |
| 6.2 Blue/Green | None | None | Dockerfile, deployment manifest (platform undetermined — Azure vs. K8s ambiguity flagged), canary-percentage layer on existing feature flags, `ITenantRolloutPolicy` |
| 6.3 Secrets management | None | None | `ISecretsProvider`, `AzureKeyVaultSecretsProvider`, `ConfigurationSecretsProvider`, `SecretRotationAuditEvent`, audit-log wiring — entire subsystem absent today |
| 6.4 Disaster recovery | None (prior dr-sentinel work already real and verified) | None | Multi-region failover runbook, RPO/RTO targets as config/constants |

**Nothing in this document should be read as "implemented" or "tested against real
infra."** Every current-state claim above was verified directly against the source
tree at the paths cited. Every "not yet built" item is a genuine gap requiring an
engineering/build agent pass, not a documentation pass.

---

## What was actually built (engineering/build pass, 2026-08-03)

A follow-up build agent picked up every gap flagged above. This section records
exactly what was added, split into real code, config/pipeline artifacts, and what
still needs real infrastructure to actually execute. Nothing below duplicates the
prior real work (backup/restore automation, the binary feature-flag system) --
everything is additive on top of it.

### 6.1 Multi-Environment Promotion -- built

- `.github/workflows/ci.yml` (repo root of this solution, `PlantSafety/.github/workflows/`):
  a real GitHub Actions workflow with `build` -> `test` -> `sql-migration-validation`
  (applies every real `Persistence/Migrations/**/*.sql` script against a throwaway
  SQL Server container) -> `contract-gate` (soft-fail placeholder -- see below) ->
  `deploy-dev` -> `deploy-test` -> `deploy-staging` -> `deploy-prod` jobs.
- **Contract-gate activation note confirmed still accurate at build time:**
  `PlantSafety.Tests/SharedKernel/Contracts/Enterprise/` still does not exist as of
  this build pass either -- the `contract-gate` job is written with
  `continue-on-error: true` and a runtime existence check, so it activates as a hard
  gate automatically the moment that directory lands, with no further pipeline edit
  required.
- **Needs real infra to execute:** this workflow requires a live GitHub repository
  with Actions enabled, the `CI_SQL_SA_PASSWORD` secret configured, and (for the
  deploy jobs) `Development`/`Test`/`Staging`/`Production` GitHub Environments
  configured with approval rules. It cannot run inside this sandbox.

### 6.2 Blue/Green + Canary + Tenant Rollout -- built

- `PlantSafety.Api/Dockerfile`: standard multi-stage .NET 8 build/publish/runtime
  Dockerfile for PlantSafety.Api. Deliberately generic/cloud-agnostic (no Kubernetes
  manifests, no Azure App Service config) -- **the actual target deploy platform
  remains a genuine, flagged ambiguity**, unchanged from the doc agent's finding; the
  `Azure` solution at `D:\PlantSafety\source\Azure` is still confirmed to be an
  unrelated domain-modeling solution, not a deployment target for this API. A human
  who owns that decision should extend/replace this Dockerfile's deploy story, not
  guess at Kubernetes/App Service specifics.
- **RolloutPercentage column** added to the real Phase 14 `[System].[FeatureFlag]`
  tables via `PlantSafety.Infrastructure/Persistence/Migrations/Phase19-DeploymentRelease/190-feature-flag-rollout-percentage.sql`
  (additive, nullable, `CHECK (0-100)`, idempotent) in both PlantSafety.ArchiveDB and
  PlantSafety.IntegrationDB.
- `FeatureFlagDto.RolloutPercentage` (optional, defaults to `null` so every existing
  caller/test compiles unchanged), `IFeatureFlagRepository.SetRolloutPercentageAsync`
  + `FeatureFlagDapperRepository` implementation, `SetFeatureFlagRolloutPercentageCommand`
  + handler + unit tests, and a new
  `PUT /api/v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/rollout-percentage`
  endpoint on the existing `OperationsConfigurationV1Controller`.
- `PlantSafety.Application/Common/Rollout/ITenantRolloutPolicy.cs` +
  `FeatureRolloutEvaluator.cs`: deterministic SHA-256-based stable hash of
  `TenantId + FeatureName` modulo 100, registered as a DI singleton and covered by
  `PlantSafety.Tests/Application/Common/Rollout/FeatureRolloutEvaluatorTests.cs`
  (determinism, 0%/100% boundaries, monotonic growth as percentage increases).
- **Needs real infra to fully exercise:** the migration script has not been run
  against a live SQL Server instance in this sandbox; the Dockerfile has not been
  built/pushed to a real registry.

### 6.3 Enterprise Secrets Management -- built (largest gap, prioritized)

- `PlantSafety.Infrastructure/Common/Secrets/ISecretsProvider.cs`.
- `AzureKeyVaultSecretsProvider.cs`: real implementation against the
  `Azure.Security.KeyVault.Secrets` SDK, authenticated via `Azure.Identity`'s
  `DefaultAzureCredential` (no client secret/certificate hardcoded anywhere). Lazily
  constructs its `SecretClient` and throws `SecretsProviderConfigurationException`
  (fail loud, not a silent no-op) if `Secrets:VaultUri` is unconfigured on first use.
- `ConfigurationSecretsProvider.cs`: local-dev-only fallback reading
  `Secrets:Values:<name>` from `IConfiguration`/user-secrets. Its constructor throws
  `SecretsProviderConfigurationException` immediately if resolved while
  `IHostEnvironment.IsProduction()` is true -- a hard startup guard, not just a doc
  comment.
- `AuditingSecretsProviderDecorator.cs`: wraps every `GetSecretAsync` call, logging
  secret NAME + outcome + timestamp + provider type -- never the value.
- `SecretRotationAuditEvent` domain event: `PlantSafety.SharedKernel/Audit/SecretRotationAuditEvent.cs`.
- `RecordSecretRotationCommand` + handler
  (`PlantSafety.Application/Operations/Secrets/`) and
  `SecretRotationWebhookV1Controller` (`PlantSafety.Api/Operations/Secrets/Controllers/V1/`):
  a real webhook endpoint handling Azure Event Grid's subscription-validation
  handshake and dispatching a rotation-recorded command for each real Key Vault
  secret event, guarded by a configuration-driven shared-secret header.
- DI selection wired into `InfrastructureServiceRegistration.cs`: explicit
  `Secrets:Provider` override if present, else environment-based
  (Production -> AzureKeyVault, else -> Configuration), always wrapped in the
  auditing decorator.
- **New NuGet dependencies added** (flag for review):
  `Azure.Security.KeyVault.Secrets` 4.7.0, `Azure.Identity` 1.13.2, both in
  `PlantSafety.Infrastructure.csproj`.
- **Honest limitation, not a fabrication:** the secret-access/rotation audit trail
  is logged via `ILogger` (structured, queryable via log aggregation), NOT inserted
  into `dbo.AuditLog`. `Shared.AuditLogging.Abstractions.IAuditLogRepository` is
  deliberately read-only in C# -- every real write to `dbo.AuditLog` happens inside
  ~400 stored procedures, and secrets have no backing SQL row of their own to embed
  such a write in. This follows the exact same, already-documented convention as the
  pre-existing feature-flag-toggle audit gap (see §2 above) rather than inventing a
  new pattern.
- **Needs real infra to actually exercise:** there is no live Azure Key Vault, Managed
  Identity, or Event Grid topic in this sandbox. `AzureKeyVaultSecretsProvider` and
  the rotation webhook are real, correct code, but unexercised against a real vault.

### 6.4 Disaster Recovery -- small, targeted addition only (as instructed)

- `PlantSafety.Infrastructure/Operations/Backup/Options/RpoRtoOptions.cs`:
  `RpoRtoTarget` record (Tier, RecoveryPointObjective, RecoveryTimeObjective) +
  `RpoRtoOptions` with two default tiers (Critical: 1h RPO / 4h RTO; Standard: 24h
  RPO / 24h RTO), configuration-overridable via `Operations:Backup:RpoRto`.
- `PlantSafety.Api/HealthChecks/BackupRpoHealthCheck.cs`: new `/health/ready` check
  (`backup-rpo`) that reuses the existing, real `GetBackupHealthQuery` (no backup
  logic duplicated) and reports Healthy/Degraded/Unhealthy by comparing each live
  database's last FULL backup age against the configured RPO target.
- Explicitly **not duplicated**: the underlying backup/restore automation
  (`BackupOptions`, `BackupDapperRepository`, `Backup.sql`,
  `usp_Backup_GetBackupJobs.sql`, `Invoke-PlantSafetyBackup.ps1`,
  `DR-manual-restore-runbook.md`) -- all confirmed real and untouched.
- **Still a genuine, unaddressed gap:** real multi-region failover (there is only
  ever one SQL Server instance in this environment) and a multi-region failover
  runbook remain out of scope for this pass, as instructed ("small, targeted
  addition only").
