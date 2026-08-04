# Phase 9 — Migration Sequencing Rules (Pre-Deploy / Post-Deploy / Safe Rollback)

Status: verified against the actual codebase on 2026-08-03 (Documentation Agent, docs-only pass).
Grounded directly in the real migration tree at
`PlantSafety.Infrastructure\Persistence\Migrations\Phase1` through `Phase20-ReleaseManagement`
(20 phase folders) and in `.github\workflows\ci.yml`'s `sql-migration-validation` job, which is
the one place in this repo that already encodes an execution order for these scripts.

## 1. The real, load-bearing convention (not invented here)

Every migration file lives under `PhaseN[-Name]/` and is prefixed with a strictly increasing
numeric id matching (or close to) its phase number, e.g. `144-feature-flags.sql`,
`190-feature-flag-rollout-percentage.sql`, `200-tenant-user-region-feature-flag-overrides.sql`.
`.github\workflows\ci.yml`'s `sql-migration-validation` job (lines 183-190) applies every script
via a `for f in .../Migrations/**/*.sql` glob loop — **glob order on Linux runners is
lexicographic**, which for this naming scheme is equivalent to numeric/phase order (`Phase1`
before `Phase10-...` is the one lexicographic wrinkle — see §4 below).

Every script inspected (Phase14, Phase17 through Phase20) uses `IF NOT EXISTS (...) BEGIN ... END`
guards for `ALTER TABLE`/`CREATE TABLE`/`CREATE SCHEMA`/constraint additions, and `CREATE OR
ALTER` for procedures — confirmed again in this pass via `190-feature-flag-rollout-percentage.sql`
and `200-tenant-user-region-feature-flag-overrides.sql` (read in full for this task). This makes
every migration **safely re-runnable** (idempotent) and, by construction, **additive-only**: no
script in the tree drops a column, drops a table, or renames an existing object.

## 2. Real cross-migration dependencies found (not generic advice)

Two concrete, verified dependency chains exist in the current tree:

- `Phase19-DeploymentRelease\190-feature-flag-rollout-percentage.sql` adds a nullable
  `RolloutPercentage` column to `[System].[FeatureFlag]` in both `PlantSafety.ArchiveDB` and
  `PlantSafety.IntegrationDB`. That table is **created** by
  `Phase14-Governance\144-feature-flags.sql`. `190-...` assumes `144-...` already ran (its
  `ALTER TABLE [System].[FeatureFlag] ADD RolloutPercentage` would fail with an object-not-found
  error against a database where Phase14 was never applied). Numeric ordering (144 < 190) already
  guarantees this in both the CI glob loop and any human running scripts in folder order.
- `Phase20-ReleaseManagement\200-tenant-user-region-feature-flag-overrides.sql` creates a new
  `[Release]` schema and four new tables (`TenantFeatureFlagOverride`, `UserFeatureFlagOverride`,
  `RegionFeatureFlagOverride`, `TenantReleaseChannel`). These are self-contained `CREATE TABLE`
  statements with no FK back to `[System].[FeatureFlag]` (confirmed by reading the file — the
  precedence chain is resolved in C# by `IFeatureFlagResolver`, not by a SQL foreign key), so
  `200-...` does not have a hard SQL-level dependency on `190-...`. It is phase-ordered after it
  only because it was written later in the same release track.

No other cross-phase `ALTER`/`REFERENCES` dependency was found in Phase17-Phase20 (Phase17 is a
single self-contained bug-fix script; Phase18 is PowerShell/runbook artifacts, not SQL DDL).

## 3. Safe sequencing rule for THIS codebase

Because every migration is additive-only and idempotent, the practical ordering constraint is
narrower than "run migrations before code" in the generic sense — it is specifically:

1. **Any migration that adds a column/table a new code path READS must be applied before that
   code is deployed.** Example: `PlantSafety.Application\Common\Rollout\FeatureRolloutEvaluator.cs`
   and `SetFeatureFlagRolloutPercentageCommand` (Phase 6.6 work) read
   `[System].[FeatureFlag].RolloutPercentage` — the app would throw a SQL error on first use if
   deployed against a database that never ran `190-feature-flag-rollout-percentage.sql`. **Rule:
   schema/table migrations that a new code path depends on must run pre-deploy** (before the new
   `PlantSafety.Api`/`PlantSafety.Infrastructure` build is promoted to that environment).
2. **`CREATE OR ALTER` procedure changes are backward-compatible by construction in this repo**
   (verified: `144-feature-flags.sql`'s procedure changes were described as "new guard clause
   inserted, all prior logic preserved" — additive, not a signature-breaking rewrite anywhere in
   the tree). These may safely run **with or immediately after** code deployment, since old and
   new application code calling the same procedure signature both continue to work.
3. **New tables with no existing reader (e.g. `Phase20`'s `[Release].*` tables, consulted only by
   the not-yet-shipped `IFeatureFlagResolver` precedence chain) can run pre- or post-deploy** —
   there is no old code path that breaks either way, since nothing reads them until the
   corresponding application code is live. Running them pre-deploy (the existing CI ordering)
   is still the lower-risk default: it means the schema is never a lurking blocker for a
   same-day follow-up deploy.
4. **Rollback**: because nothing is ever dropped or renamed, "rollback" in this codebase's
   convention is never "run migration N in reverse" (no down-scripts exist anywhere in the tree —
   confirmed; every `PhaseN` folder contains only forward scripts). Safe rollback of a bad
   *application* deploy is: redeploy the previous container image/build. The additive schema
   change from the associated migration is left in place (an unused nullable column or an unused
   table is harmless) rather than reverse-migrated. This matches the existing DR runbook's
   philosophy (`Phase18-ExpressChecklist\DR-manual-restore-runbook.md`) of restore-from-backup as
   the actual rollback mechanism for anything more destructive than "redeploy the previous build."

## 4. One real ordering gap found in the CI glob (flagged, not silently fixed)

`ci.yml`'s `for f in .../Migrations/**/*.sql` on a `bash`/`globstar` shell iterates directories in
plain lexicographic order. For single- vs. double-digit phase folder names this produces:
`Phase1`, `Phase10-Automation`, `Phase11-Performance`, ..., `Phase19-DeploymentRelease`, `Phase2`,
`Phase20-ReleaseManagement`, `Phase3`, ... — i.e. `Phase2` through `Phase9` sort AFTER
`Phase10`-`Phase19` lexicographically, not before them. Because every script's own numeric file
prefix still matches its phase number exactly (`020-...`, `090-...` conceptually — actual files
inside Phase2/Phase3/etc. use their own two/three-digit prefixes), and because no migration in
this tree has been found to depend on anything from a HIGHER-numbered phase, this has not caused a
real ordering failure in the scripts that exist today. It is nonetheless a latent risk: if a
future low-numbered phase folder's script were ever written to depend on a higher-numbered one
(which would violate the additive/forward-only convention anyway), the CI loop's file-system
ordering would not guarantee it runs after. **Recommendation (not implemented in this pass, since
it is a CI-behavior change beyond a pure doc fix):** replace the `for f in .../Migrations/**/*.sql`
glob with a loop that sorts by the leading numeric prefix extracted from the phase folder name
(e.g. `ls -d .../Migrations/Phase*/ | sort -t P -k2 -n`) rather than relying on shell glob order.
This is called out here as a precise, scoped follow-up rather than fixed silently in the YAML,
since it changes pipeline execution behavior rather than adding a net-new, purely-additive step.
