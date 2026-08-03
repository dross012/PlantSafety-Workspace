# Compliance Documentation Index (Phase 7 §6.4)

Pointer index to the real compliance/security documentation already produced in this and prior
phases. This file does not restate their content — it exists so the Admin Portal / onboarding
material has one canonical place to link from.

## Phase 7 documents

- `docs\Phase7\security-compliance.md` — Phase 7 security & compliance posture.
- `docs\Phase7\compliance-framework-mapping.md` — control-to-framework mapping.
- `docs\Phase7\tenant-isolation-model.md` — multi-tenant data isolation model (directly relevant
  to the Admin Portal's Tenant Management and the `X-Tenant-ID` / impersonation mechanism —
  see `docs\Phase7\api-documentation-status.md` for how impersonation's header override
  interacts with `TenantResolutionMiddleware`).
- `docs\Phase7\multi-region-architecture.md` / `docs\Phase7\multi-region-dr-runbook.md` — Global
  Scale (data residency, region pinning — the `Region` field on `TenantDto` surfaced in
  `PlantSafety.UI\Modules\Admin\Pages\TenantDetailPage.razor`).
- `docs\Phase7\versioning-conventions.md` — API versioning conventions (`Asp.Versioning`,
  `[ApiVersion("1.0")]`, applied uniformly including this session's new
  `BillingV1Controller`/`ImpersonationV1Controller`/`ComplianceMonitoringV1Controller`).

## Phase 6 documents

- `docs\Phase6\deployment-release-architecture.md` — release/deployment architecture, the
  foundation the Phase 7 §7.2 tenant release-channel/rollback feature
  (`OperationsConfigurationV1Controller`'s `SetTenantReleaseChannelAsync`/`TriggerRollbackAsync`,
  surfaced in the extended `/modules/operations-configuration` page) builds on.

## Phase 5 documents

- `docs\Phase5\phase5-event-flow.md` — event-flow documentation, relevant background for the
  Enterprise Event Mesh / Integrations layer referenced (but not duplicated) by the Support
  Console.

## Audit trail mechanisms referenced by this session's new Support Tooling

- `PlantSafety.Application\Auditing\ComplianceMonitoring\PermissionUsage\Models\PermissionUsageLogEntry.cs`
  — the existing authorization-decision audit log. This session's
  `StartImpersonationCommandHandler`/`EndImpersonationCommandHandler`
  (`PlantSafety.Application\Support\Impersonation\Handlers\`) write real entries here
  (`Support.Impersonation.Start` / `Support.Impersonation.End`) rather than a new, parallel
  logging mechanism.
- `PlantSafety.Api\Auditing\ComplianceMonitoring\Controllers\V1\ComplianceMonitoringV1Controller.cs`
  (new this session) — the first HTTP surface for `GetTenantAuditStreamQuery`, consumed by the
  Support Console's audit stream viewer.
