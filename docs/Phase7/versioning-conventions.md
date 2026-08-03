# Phase 7 — Versioning Conventions (SaaS Productization Blueprint §7.3)

Status: verified against the actual codebase on 2026-08-03. This document consolidates the
five versioning dimensions called for by §7.3. Three (APIs, Events, Data Contracts) were
already real and are verified here, not rebuilt. One (Stored Procedures) has a real,
pre-existing migration-numbering convention that is documented here rather than replaced.
One (UI Components) has **no** existing convention — that is a genuine gap, called out below
as a recommendation rather than a fabricated scheme.

## 1. APIs — verified, pre-existing

Convention: ASP.NET Core API versioning via the `[ApiVersion("x.0")]` attribute plus a
`V{n}` controller-folder/class-name suffix and a versioned route segment.

Example (verified in the codebase):

```
PlantSafety.Api/Operations/Configuration/Controllers/V1/OperationsConfigurationV1Controller.cs
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/operations/configuration")]

PlantSafety.Api/Controllers/V1/PlatformV1Controller.cs
PlantSafety.Api/Controllers/V2/PlatformV2Controller.cs
PlantSafety.Api/Analytics/Dashboard/TrendAnalysis/Controllers/V1/TrendAnalysisV1Controller.cs
PlantSafety.Api/Analytics/Dashboard/TrendAnalysis/Controllers/V2/TrendAnalysisV2Controller.cs
```

Dozens of modules follow this same `Controllers/V1/{Module}V1Controller.cs`,
`Controllers/V2/{Module}V2Controller.cs` pattern side by side, allowing a V1 and V2
controller for the same module to coexist and be independently deprecated. The new Phase 7
Tenant/User/Region feature flag override + rollback endpoints added by this task
(`OperationsConfigurationV1Controller`) follow the identical convention — no new endpoints
were added to a V2 controller since they are additive extensions of the existing V1 surface.

**No changes needed.** This convention is consistent and does not need rebuilding.

## 2. Events — verified, pre-existing

Convention: every event on the Enterprise Event Mesh carries an explicit
`EnterpriseEventEnvelope.SchemaVersion` (int), and an `IEventUpgrader`/`EventUpgradePipeline`
performs versioned, additive upgrades from an older schema version to the current one before
the event reaches a consumer.

Verified files:

```
PlantSafety.SharedKernel/Contracts/Enterprise/EventMesh/EnterpriseEventEnvelope.cs   (SchemaVersion: 2026.2)
PlantSafety.SharedKernel/Contracts/Enterprise/EventMesh/IEventUpgrader.cs
PlantSafety.SharedKernel/Contracts/Enterprise/EventMesh/EventUpgradePipeline.cs
PlantSafety.SharedKernel/Contracts/Enterprise/EventMesh/DeprecatedEventVersionRegistry.cs
```

`DeprecatedEventVersionRegistry` tracks deprecated `(EventType, SchemaVersion)` pairs so
producers/consumers can be warned off an old shape while the upgrade pipeline still services
it. **No changes needed.**

## 3. Data Contracts — verified, pre-existing

Convention: `CanonicalX` enterprise-integration DTOs (ERP/MES/CMMS adapters, event mesh) are
versioned as `CanonicalXV1`, `CanonicalXV2`, `CanonicalXV3` — each version additive-only over
its predecessor (never breaking), with an explicit `public const int SchemaVersion = N;` on
each record.

Verified files (all under `PlantSafety.SharedKernel/Contracts/Enterprise/CanonicalModel/`):

```
CanonicalAsset.cs             (CanonicalAssetV1 / V2 / V3)
CanonicalWorkOrder.cs
CanonicalProductionOrder.cs
CanonicalRiskEvent.cs
CanonicalImprovementAction.cs
CanonicalSchedulingBlock.cs
```

Round-trip mapping, schema-diff, and version-upgrade behavior are covered by
`PlantSafety.Tests/SharedKernel/Contracts/Enterprise/CanonicalModel*Tests.cs`
(`CanonicalModelMappingRoundTripTests`, `CanonicalModelSchemaDiffTests`,
`CanonicalModelSnapshotTests`, `CanonicalModelVersioningTests`). **No changes needed.**

## 4. Stored Procedures / SQL Migrations — verified, pre-existing convention documented here

There is no per-procedure `usp_Foo_V2` naming scheme. Instead, the real convention is a
**migration-numbered folder + file-prefix scheme** applied to the entire
`PlantSafety.Infrastructure/Persistence/Migrations/` tree: each phase of work gets its own
`PhaseN[-Name]/` folder, and every script inside is prefixed with a strictly increasing
numeric migration id matching (or close to) the phase number, e.g.:

```
Phase14-Governance/144-feature-flags.sql
Phase18-ExpressChecklist/180-database-size-log.sql
Phase19-DeploymentRelease/190-feature-flag-rollout-percentage.sql
Phase20-ReleaseManagement/200-tenant-user-region-feature-flag-overrides.sql   <- added by this task
```

Within a single object's lifetime, procedures are evolved via `CREATE OR ALTER` in a later
migration script (never a parallel `_V2` procedure) — e.g. `144-feature-flags.sql` altered
5 pre-existing retention-purge procedures additively (new guard clause inserted, all prior
logic preserved) rather than creating new versioned copies. Column-level schema evolution on
a table (e.g. adding `RolloutPercentage` to `[System].[FeatureFlag]` in
`190-feature-flag-rollout-percentage.sql`, or adding the four new `[Release]` schema tables
in `200-tenant-user-region-feature-flag-overrides.sql`) is done via idempotent,
existence-guarded `ALTER TABLE`/`CREATE TABLE` statements in a new, higher-numbered migration
file — never by editing an already-shipped migration script in place.

This is the real, load-bearing convention (confirmed by reading the full migration tree,
19 phase folders, Phase1 through Phase19 prior to this task's Phase20 addition). It is
documented here rather than replaced with a fabricated `usp_X_V2` scheme, per the task's
"verify, don't invent" instruction.

## 5. UI Components (Blazor Hybrid) — genuine gap, no existing convention

A full search of `PlantSafety.UI` (Components, Modules, Pages, `.razor` files, `AssemblyInfo`)
found **no** per-component versioning marker of any kind: no `ComponentVersion` attribute, no
version suffix in component file/class names (no `FooV1.razor` / `FooV2.razor` pairs
anywhere), no version comment header convention analogous to the `SchemaVersion: 2026.1`
comment blocks used throughout the Application/Infrastructure C# layers. The handful of
`grep` hits for version-shaped strings in `PlantSafety.UI` were incidental (an API client
model's own `SchemaVersion` DTO field mirroring a versioned API response, and
auto-generated `AssemblyInfo.cs`/`AppxManifest.xml` build artifacts) — not a real
component-versioning scheme.

This is consistent with the nature of a Blazor Hybrid client: it is normally rebuilt and
redeployed as a single unit alongside each release, so individual `.razor` components have
not needed independent version identity so far.

**Recommendation (not fabricated as if it already existed):** if/when the UI needs to
support multiple concurrently-live component shapes (e.g. a Beta-channel tenant seeing a
new component driven by a Phase 7 `TenantReleaseChannel`/`IFeatureFlagResolver` decision
while a Stable-channel tenant sees the old one), the lowest-risk convention consistent with
the rest of this codebase would be:

1. Co-locate both shapes under the same feature folder with an explicit suffix, mirroring
   the API's `V1`/`V2` convention: `Components/Pages/FeatureFlagsDashboard/
   FeatureFlagsDashboardPageV1.razor` and `...V2.razor`, selected at runtime by resolving
   the relevant `ResolveFeatureFlagQuery` result (via the existing
   `IFeatureFlagResolver`/API endpoint added in this task) rather than a build-time switch.
2. Add a short XML-doc-style comment header (matching the C#
   `// SchemaVersion: N` convention already used everywhere else in this codebase) to the
   top of each versioned `.razor` file, so `grep`-based discovery of "what UI version
   exists" stays consistent with how every other layer documents its own version.

No code changes were made under `PlantSafety.UI` for this recommendation — it is
speculative future guidance only, not a currently-enforced rule.
