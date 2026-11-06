# PlantSafety FY2027 Platform — Status

No repo-wide status tracker existed before this file; completion state had to
be re-derived module-by-module each time it was asked about. This file exists
to stop that. Update it whenever a module's state changes materially — don't
let it go stale the way its absence let this exact question get re-asked and
re-audited from scratch.

Last updated: 2026-09-07 (GovernanceExt governance pass).

## How to read this

- **Done** — builds clean, tests pass, and (for SQL modules) deployed/verified
  live where applicable. No known open work.
- **Partial** — real, non-trivial content exists, but something concrete is
  missing, disabled, or unverified. The gap is named, not vague.
- **Not started** — placeholder/empty scaffolding only, or spec docs with no
  implementation.

A module being **Done** here means "this piece is finished," not "the whole
platform is finished" — see the Known gaps section at the bottom.

## Done

| Module | Location | Notes |
|---|---|---|
| PlantSafety (core app) | `PlantSafety/` (own repo, `dross012/PlantSafety`, branch `master`) | Mature: full API/Domain/Application/Infrastructure/UI + ~15 test projects. |
| PlantSafety.UI | `PlantSafety/PlantSafety.UI*` | Full app, wired. |
| SafetyIntegrations | `SafetyIntegrations/` (own repo, `dross012/SafetyIntegrations`) | Four-layer integration fabric (contract/validation/orchestration/execution). Verified 2026-09-06: builds clean, **402/402 tests passing**. Clean git state, no divergence from its own origin. |
| SafetyTooling | `SafetyTooling/` (own repo, `dross012/SafetyTooling`) | Roslyn analyzer/codefix/generator/dependency-rule/doc tooling suite. Verified 2026-09-06: builds clean, **774/774 tests passing** across 21 test assemblies. Clean git state, no divergence from its own origin. |
| Governance (PlatformServices) | `D:\Manufacturing Program\PlatformServices\Governance` | `tests/VALIDATION_REPORT.md` — zero findings across 198 procs/64 tables/19 views/14 functions/21 triggers. |
| PlantDigitalTwin | `D:\Manufacturing Program\PlatformServices\PlantDigitalTwin` (own repo, `dross012/PlantDigitalTwin`) | Full SQL module + .NET solution, GovernanceExt-backed auth verified live, replay/timestamp protections, error-code registry. Separate module from PlantSafety proper. |
| `EmergencyMgt-Integration` | `Enterprise/Integration/sql/EmergencyMgt-Integration` | Deployed live to `PlantSafety.CoreDB`, `usp_ValidateEmergencyMgtIntegration`: 23/23. Has a full `/integration/emergency/*` API vertical slice. |
| `IncidentMgt-Integration` | `Enterprise/Integration/sql/IncidentMgt-Integration` | Deployed live 2026-09-06, `usp_ValidateIncidentMgtIntegration`: 44/44. Has a full `/integration/incident/*` API vertical slice (10 endpoints), smoke-tested live. |
| `Shared.EventBus` | `Shared/EventBus/` | Reconstructed after being declared unrecoverable in a 2026-08-29 PlantSafety build-repair pass, tested in isolation (41/41). Re-wired into PlantSafety 2026-09-07: `ProjectReference`, DI registration, and the `CrossModuleDomainEventMapper[V2]`/`CrossModuleEventRelay[V2]` tests restored. Verified: full solution builds 0 errors, full test suite 3861/3861. See `Shared/EventBus/docs/README.md` and `PlantSafety/docs/architecture/eventbus-architecture.md`. |
| IdentityExt | `D:\Manufacturing Program\PlatformServices\IdentityExt` (own repo, `dross012/IdentityExt`) | Full 5-project stack (Domain/Application/Infrastructure/API/Client), 151 SQL files. First real governance/validation pass 2026-09-07 (previously "probably fine", not verified) — engineering judged genuinely solid, not zero-findings: fixed a real DENY-SELECT vs. raw-SQL contradiction (9 call sites bypassing IdentityExt_App's own access-control model), added SecurityEvent to the retention purge (was persisting IP addresses indefinitely with no erasure path), gated 5 previously-ungated privilege-escalation commands, added MFA re-enroll 409 + failed-verification lockout, fixed startup silently disabling auth outside Development, and corrected stale compliance docs. See `IdentityExt/Claude.md`'s "Governance/validation pass (2026-09-07)" section for full detail. Verified: 325/325 tests, live-deployed SQL. Two items deliberately left open, not fixed (flagged, not silently decided): read/list endpoints (incl. the security-events feed) remain ungated by convention; the MFA subject-ownership authorization gap (any authenticated caller can act on another subject's SubjectPrincipalGuid) is a larger design question. |
| GovernanceExt | `D:\Manufacturing Program\PlatformServices\GovernanceExt` (own repo, `dross012/GovernanceExt`) | Second Platform Spine module audited (Phase 1 of the PlatformServices governance sequence: IdentityExt → **GovernanceExt** → IntegrationHub). API + Blazor Client + Worker + a large SQL layer (69 tables, 364 procedures, no Domain/Application/Infrastructure split — a deliberate, consistently-applied style). The SQL layer itself was the strongest seen in this program (364/364 procs with TRY/CATCH + centralized error logging, zero unindexed FKs, 132/132 live SQL tests), but the audit found two serious, real authorization holes: (1) a systemic cross-tenant write bypass — every Update/SoftDelete endpoint (27 generated controllers plus 2 hand-written) had a tenant check on reads only, so a tenant-scoped caller could modify or delete another tenant's row by id, and one endpoint let a caller re-home an extension across tenants outright; fixed at the generator root cause, regenerated, live-verified with a real blocked attack (404, record unchanged); (2) `PlantDigitalTwin.Api` held full Admin in the live database when it only needed one read-shaped permission — revoked live, replaced with a new minimal `IntegrationReader` role, live-verified PDT's actual integration still works. Also fixed: `ExtensionPrincipal.IsActive` was stored but never enforced (deactivating a principal did nothing) plus a silent-reactivation bug in the update proc; a cryptographic seal endpoint gated on a read permission; two exception-message leaks (one to unauthenticated callers); a missing retention policy for `SecurityEvent` (IP addresses persisted indefinitely); and the same startup fail-open gap IdentityExt had. Verified: 132/132 SQL tests, 6/6 API tests, full solution builds. |

## Partial

| Module | Location | What's missing/open |
|---|---|---|
| `Shared.Authorization` | `Shared/Authorization/` | Real, tested (100/100), actively consumed by PlantSafety. Committed 2026-09-06 (was previously untracked despite committed PlantSafety code depending on it — see `PHASE9`–`PHASE13`-README.md for the modernization history). No known open gap beyond normal ongoing phase work. |
| `Shared.AuditLogging` | `Shared/AuditLogging/` | Real, tested (13/13), actively consumed by PlantSafety + (historically) Reporting. Committed 2026-09-06 for the same reason as Authorization. No known open gap. |
| Most mid-size PlatformServices modules | `D:\Manufacturing Program\PlatformServices\{Azure,Diagnostics,Reporting,IntegrationHub,OpsSpine,DataLake,Intelligence,Monitoring,ExportImport}` | Substantial real code, but none have been run through a validation/governance report the way PlantDigitalTwin, Governance, IdentityExt, and (as of 2026-09-07) GovernanceExt were. Status is "probably fine" not "verified." Per the governance sequence, IntegrationHub is next (Platform Spine phase 3). |

## Not started

| Module | Location | Notes |
|---|---|---|
| DRBackup, PlantAI, PlatformCaching, PlatformConfiguration, PlatformMessaging, PlatformObservability, PlatformOrchestration, PlatformPerformance, PlatformScheduling, PlatformSecrets, SecProbeCorrHarness | `D:\Manufacturing Program\PlatformServices\*` | Empty scaffolding, zero files. |
| SafetySpine, PredictiveSpine | `D:\Manufacturing Program\PlatformServices\*` | Spec `.docx` only, no implementation. |

## Known gaps (not module-specific)

- This file is new and manually maintained — nothing enforces it staying accurate. Update it as part of any change that moves a module between buckets.
- `sql/DEPLOYMENT-MANIFEST.md` remains the source of truth for SQL deploy order/tiers specifically; this file is the broader cross-module picture and doesn't duplicate that detail.
- Several modules listed above as their own git repos (`PlantSafety/`, `SafetyIntegrations/`, `SafetyTooling/`) are nested inside this outer repo's working tree but are **not** git submodules — the outer repo's `git status` shows them as untracked directories. This is intentional (each has its own remote and history) but easy to misread as "uncommitted work" — check the nested repo's own `git status`/`git log` before assuming that.
