# PlantSafety FY2027 Platform — Status

No repo-wide status tracker existed before this file; completion state had to
be re-derived module-by-module each time it was asked about. This file exists
to stop that. Update it whenever a module's state changes materially — don't
let it go stale the way its absence let this exact question get re-asked and
re-audited from scratch.

Last updated: 2026-09-07 (IdentityExt governance pass).

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

## Partial

| Module | Location | What's missing/open |
|---|---|---|
| `Shared.Authorization` | `Shared/Authorization/` | Real, tested (100/100), actively consumed by PlantSafety. Committed 2026-09-06 (was previously untracked despite committed PlantSafety code depending on it — see `PHASE9`–`PHASE13`-README.md for the modernization history). No known open gap beyond normal ongoing phase work. |
| `Shared.AuditLogging` | `Shared/AuditLogging/` | Real, tested (13/13), actively consumed by PlantSafety + (historically) Reporting. Committed 2026-09-06 for the same reason as Authorization. No known open gap. |
| Most mid-size PlatformServices modules | `D:\Manufacturing Program\PlatformServices\{Azure,Diagnostics,Reporting,GovernanceExt,IntegrationHub,OpsSpine,DataLake,Intelligence,Monitoring,ExportImport}` | Substantial real code, but none have been run through a validation/governance report the way PlantDigitalTwin, Governance, and (as of 2026-09-07) IdentityExt were. Status is "probably fine" not "verified." |

## Not started

| Module | Location | Notes |
|---|---|---|
| DRBackup, PlantAI, PlatformCaching, PlatformConfiguration, PlatformMessaging, PlatformObservability, PlatformOrchestration, PlatformPerformance, PlatformScheduling, PlatformSecrets, SecProbeCorrHarness | `D:\Manufacturing Program\PlatformServices\*` | Empty scaffolding, zero files. |
| SafetySpine, PredictiveSpine | `D:\Manufacturing Program\PlatformServices\*` | Spec `.docx` only, no implementation. |

## Known gaps (not module-specific)

- This file is new and manually maintained — nothing enforces it staying accurate. Update it as part of any change that moves a module between buckets.
- `sql/DEPLOYMENT-MANIFEST.md` remains the source of truth for SQL deploy order/tiers specifically; this file is the broader cross-module picture and doesn't duplicate that detail.
- Several modules listed above as their own git repos (`PlantSafety/`, `SafetyIntegrations/`, `SafetyTooling/`) are nested inside this outer repo's working tree but are **not** git submodules — the outer repo's `git status` shows them as untracked directories. This is intentional (each has its own remote and history) but easy to misread as "uncommitted work" — check the nested repo's own `git status`/`git log` before assuming that.
