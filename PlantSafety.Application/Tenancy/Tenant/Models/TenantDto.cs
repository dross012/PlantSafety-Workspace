// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical read model for the Tenant aggregate (Phase 7 §2.1 Automated Tenant
//   Creation).
// SchemaVersion: 1

namespace PlantSafety.Application.Tenancy.Tenant.Models;

/// <summary>Canonical read model for a registered platform tenant.</summary>
/// <param name="TenantId">The tenant's own strong-typed identity.</param>
/// <param name="Name">The tenant's registered display name.</param>
/// <param name="Region">Data-residency/deployment region the tenant is pinned to.</param>
/// <param name="Status">Current lifecycle status code (Provisioning/Active/Suspended/Archived/Deleted).</param>
/// <param name="PlanCode">Subscription plan reference code.</param>
/// <param name="Configuration">The tenant's own configuration.</param>
/// <param name="CreatedAtUtc">UTC instant the tenant was registered.</param>
/// <param name="ActivatedAtUtc">UTC instant provisioning completed, if any.</param>
/// <param name="SuspendedAtUtc">UTC instant of the most recent suspension, if any.</param>
/// <param name="SuspensionReason">Reason recorded for the most recent suspension, if any.</param>
/// <param name="ArchivedAtUtc">UTC instant the tenant's data was archived, if any.</param>
/// <param name="DeletedAtUtc">UTC instant the tenant was soft-deleted, if any.</param>
public sealed record TenantDto(
    Guid TenantId,
    string Name,
    string Region,
    string Status,
    string PlanCode,
    TenantConfigurationDto Configuration,
    DateTime CreatedAtUtc,
    DateTime? ActivatedAtUtc,
    DateTime? SuspendedAtUtc,
    string? SuspensionReason,
    DateTime? ArchivedAtUtc,
    DateTime? DeletedAtUtc);
