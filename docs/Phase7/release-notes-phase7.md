# Phase 7 Release Notes — SaaS Productization Blueprint

This document summarizes what was actually built during the Phase 7 session, grounded in files
that exist in the repository at the time of writing. It does not describe planned/aspirational
work beyond what is implemented.

## §2 Tenancy — Automated Tenant Creation & Lifecycle Management

- `ProvisionTenantCommand`/`ProvisionTenantCommandHandler`
  (`PlantSafety.Application\Tenancy\Tenant\Commands\`) — end-to-end tenant provisioning: creates
  the Tenant aggregate, the initial administrative user
  (`ITenantAdminUserProvisioner`), assigns roles (`ITenantRoleAssigner`), and generates a
  primary API key (`ITenantApiKeyGenerator`).
- Lifecycle commands: `SuspendTenantCommand`, `ReactivateTenantCommand`, `DeleteTenantCommand`
  (soft-delete), `ArchiveTenantDataCommand`, `ExportTenantDataCommand`
  (blob-backed via `ITenantDataExportStore`/`TenantDataExportStore`).
- Read side: `GetTenantQuery`/`GetTenantListQuery` via `ITenantReadRepository`.
- API surface: `PlantSafety.Api\Tenancy\Controllers\V1\TenantV1Controller.cs` — every action
  gated at `TenantAuthorizationPolicies.Manage` (PlatformAdmin-and-above).

## §3 Billing — Usage Metering, Subscription Plans, Billing Engine

- Subscription plan resolution: `GetSubscriptionPlanForTenantQuery`
  (`PlantSafety.Application\Billing\Subscription\`).
- Usage metering composition: `GetTenantUsageMeteringSummaryQuery` /
  `CaptureUsageMeteringSnapshotCommand` (`PlantSafety.Application\Billing\Metering\`), composed
  from existing platform data sources rather than a new metering pipeline.
- Invoice lifecycle: `GenerateInvoiceCommand`, `ChargeInvoiceCommand` (via the
  `IPaymentProcessor` extension seam), `RefundInvoiceCommand`
  (`PlantSafety.Application\Billing\BillingEngine\`).
- **Gap found and closed this session:** the Billing Application layer had no corresponding Api
  controller. Added `PlantSafety.Api\Billing\Controllers\V1\BillingV1Controller.cs` (thin HTTP
  boundary only, no business logic) plus its authorization policy
  (`PlantSafety.Api\Billing\Authorization\BillingAuthorizationPolicies.cs`) and request contracts
  (`PlantSafety.Api\Billing\Contracts\Requests\BillingRequests.cs`).

## §5 Auditing / Compliance Monitoring

- `GetTenantAuditStreamQuery`/`GetTenantAuditStreamQueryHandler`
  (`PlantSafety.Application\Auditing\ComplianceMonitoring\PermissionUsage\`) — tenant-scoped
  access-log/permission-usage stream, composed over the existing `AuthorizationBehavior`
  capture pipeline.
- **Gap found and closed this session:** no Api controller exposed this query. Added
  `PlantSafety.Api\Auditing\ComplianceMonitoring\Controllers\V1\ComplianceMonitoringV1Controller.cs`
  (single `GET tenants/{tenantId}/audit-stream` endpoint, gated at Approver-and-above).

## §6 Productization & Customer Experience (this agent's primary scope)

### §6.1 Admin Portal (`PlantSafety.UI\Modules\Admin\`)

- Tenant management: `TenantListPage.razor`, `TenantDetailPage.razor` (lifecycle actions:
  suspend/reactivate/archive/export/soft-delete), `TenantProvisionPage.razor` (guided wizard
  calling `ProvisionTenantCommand`).
- Billing tab (inside `TenantDetailPage.razor`): subscription plan, usage-metering summary,
  invoice list with generate/charge/refund actions — calling the new `BillingV1Controller`.
- Feature flags: extended the existing `/modules/operations-configuration` page
  (`ConfigurationDashboardPage.razor`) with rollout-percentage, tenant/user/region override,
  release-channel, and rollback forms — calling `OperationsConfigurationV1Controller`'s Phase 7
  §7.1/§7.2 endpoints. Did not duplicate the existing feature-flag toggle UI.
- Module configuration: tenant detail's Overview tab surfaces `TenantConfiguration.EnabledModules`.

### §6.2 Customer Onboarding

- Guided setup: `TenantProvisionPage.razor` — a minimal 3-step wizard around the real
  `ProvisionTenantCommand`, not a scripted product-tour engine.
- Quick-start checklist: `OnboardingChecklistPage.razor` (`/admin/onboarding`) — navigation
  scaffolding only, linking to real existing pages (Incident Management, Safety Programs,
  Administrative Setup, Feature Flags, Support Console). No new business logic.
- Sample-data seeding: `ProvisionTenantCommandHandler` already seeds default reference data as
  part of the provisioning pipeline (per its own file header). No additional seeding pipeline
  was built this session beyond documenting this in the checklist page as a recommendation for
  future work.

### §6.3 Support Tooling

- Support Console: `SupportConsolePage.razor` (`/admin/support`) — tenant list with quick links,
  links to the existing Platform Health Dashboard (`/modules/operations-system-health`),
  cross-database Audit Viewer (`/modules/operations-audit-viewer`), and Enterprise Fabric
  dashboard (`/modules/enterprise-fabric`) rather than rebuilding those dashboards.
- **Tenant impersonation (audited, real, new this session):**
  - Application layer: `PlantSafety.Application\Support\Impersonation\` —
    `StartImpersonationCommand`/`EndImpersonationCommand`/`GetActiveImpersonationSessionQuery`
    and their handlers. Sessions are stored in the existing Redis-backed
    `PlantSafety.Application.Abstractions.Caching.ICacheService` with a hard 30-minute
    time-box (`StartImpersonationCommandHandler.SessionTimeBox`) — the session cannot outlive
    its cache TTL even if never explicitly ended.
  - Every session start/end writes a real audit entry to the existing
    `IPermissionUsageLogRepository` (`Support.Impersonation.Start` / `Support.Impersonation.End`
    permission names), queryable via the same `GetTenantAuditStreamQuery` used by the Support
    Console's audit stream viewer.
  - Api layer: `PlantSafety.Api\Support\Impersonation\Controllers\V1\ImpersonationV1Controller.cs`,
    gated at `ImpersonationAuthorizationPolicies.Manage` (PlatformAdmin-and-above, auto-registered
    by the existing `AuthorizationPolicyExtensions` reflection-based discovery).
  - UI: `TenantDetailPage.razor`'s Support tab and `SupportConsolePage.razor` both expose
    start/end actions; `ImpersonationBanner.razor` (rendered from `MainLayout.razor`) is a
    persistent, always-visible banner shown on every page while a session is active, with a
    live countdown to `ExpiresAtUtc` and an explicit End Session button.
  - `ImpersonationHeaderHandler.cs`
    (`PlantSafety.UI\Infrastructure\Impersonation\`) stamps the `X-Tenant-ID` header (already
    honored by `TenantResolutionMiddleware`) on every outbound API request while a session is
    active, registered once in the shared `RegisterApiClient` handler pipeline in
    `MauiProgram.cs` so every existing typed API client in the app picks it up automatically.
- Log/event viewer: the Support Console's audit stream panel composes the real
  `GetTenantAuditStreamQuery`. A dedicated Enterprise Event Mesh (Outbox/DeadLetterEvents)
  visual viewer was **not** built this session (documented as a gap in
  `docs\Phase7\module-docs-index.md`) — the console links to the existing Enterprise Fabric
  dashboard instead of duplicating that infrastructure.
- Health dashboards: reused, not rebuilt — Support Console links directly to
  `/modules/operations-system-health`.

### §6.4 Documentation

- `docs\Phase7\api-documentation-status.md` — confirms live Swagger/OpenAPI and documents how
  to access it.
- `docs\Phase7\module-docs-index.md` — real index of every `PlantSafety.Api` bounded context,
  generated from the actual controller folder tree.
- `docs\Phase7\compliance-docs-index.md` — pointer index to the real Phase 6/7 compliance docs.
- This file.

## Verification performed

- `dotnet build PlantSafety.Application/PlantSafety.Application.csproj` — 0 errors.
- `dotnet build PlantSafety.Api/PlantSafety.Api.csproj` — 0 errors (includes the new
  `BillingV1Controller`, `ImpersonationV1Controller`, `ComplianceMonitoringV1Controller`, and
  their supporting Application-layer commands/queries/handlers).
- `PlantSafety.UI` could not be compiled in this environment — the `net8.0-android`,
  `net8.0-ios`, and `net8.0-maccatalyst` MAUI workloads are unavailable (confirmed by multiple
  prior agents this session; `dotnet build` fails at restore with `NETSDK1202`/`NU1015` before
  any C#/Razor compilation is attempted). All new UI files were instead verified structurally:
  every new `ApiClient`/`State` class registration was cross-checked against
  `PlantSafety.UI\MauiProgram.cs`, every new Razor page's injected services/component
  parameters were checked against the real shared-component signatures
  (`DataGridComponent`, `CardComponent`, `TabControlComponent`, `StatusIndicatorComponent`,
  `ErrorBoundaryComponent`, `LoadingIndicatorComponent`), and every new route constant was
  added to `PlantSafety.UI\Routing\RouteDefinitions.cs`.
