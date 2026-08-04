// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal's Tenant Management pages (Phase 7
//   §6.1 Admin Portal). Mirrors PlantSafety.Application.Tenancy.Tenant.Models.* and the
//   TenantV1Controller request contracts so TenantAdminApiClient can deserialize/serialize
//   real backend shapes directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>A single tenant-scoped feature-flag override (mirrors <c>TenantFeatureFlagOverrideDto</c>).</summary>
public sealed class TenantConfigFeatureFlagOverrideDto
{
    /// <summary>Gets the overridden flag's identifier.</summary>
    public string FlagName { get; init; } = string.Empty;

    /// <summary>Gets the tenant-specific enabled/disabled value.</summary>
    public bool IsEnabled { get; init; }
}

/// <summary>A tenant's configuration child entity (mirrors <c>TenantConfigurationDto</c>).</summary>
public sealed class TenantConfigurationDto
{
    /// <summary>Gets the identifiers of optional platform modules enabled for this tenant.</summary>
    public IReadOnlyList<string> EnabledModules { get; init; } = [];

    /// <summary>Gets the tenant-scoped feature-flag overrides.</summary>
    public IReadOnlyList<TenantConfigFeatureFlagOverrideDto> FeatureFlagOverrides { get; init; } = [];

    /// <summary>Gets the custom Compliance.RulesEngine rule codes this tenant has opted into.</summary>
    public IReadOnlyList<string> CustomComplianceRuleCodes { get; init; } = [];
}

/// <summary>Canonical read model for a registered platform tenant (mirrors <c>TenantDto</c>).</summary>
public sealed class TenantAdminDto
{
    /// <summary>Gets the tenant's own strong-typed identity.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the tenant's registered display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the data-residency/deployment region the tenant is pinned to.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status code (Provisioning/Active/Suspended/Archived/Deleted).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the subscription plan reference code.</summary>
    public string PlanCode { get; init; } = string.Empty;

    /// <summary>Gets the tenant's own configuration.</summary>
    public TenantConfigurationDto Configuration { get; init; } = new();

    /// <summary>Gets the UTC instant the tenant was registered.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Gets the UTC instant provisioning completed, if any.</summary>
    public DateTime? ActivatedAtUtc { get; init; }

    /// <summary>Gets the UTC instant of the most recent suspension, if any.</summary>
    public DateTime? SuspendedAtUtc { get; init; }

    /// <summary>Gets the reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; init; }

    /// <summary>Gets the UTC instant the tenant's data was archived, if any.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the tenant was soft-deleted, if any.</summary>
    public DateTime? DeletedAtUtc { get; init; }
}

/// <summary>Outcome of a completed tenant-provisioning pipeline run.</summary>
public sealed class TenantProvisioningResultDto
{
    /// <summary>Gets the newly provisioned (now Active) tenant's read model.</summary>
    public TenantAdminDto Tenant { get; init; } = new();

    /// <summary>Gets the identity of the tenant's newly created administrative user.</summary>
    public Guid AdminUserId { get; init; }

    /// <summary>Gets the tenant's newly generated primary API key, in plaintext -- shown exactly once.</summary>
    public string ApiKey { get; init; } = string.Empty;
}

/// <summary>Outcome of a completed tenant data export.</summary>
public sealed class TenantDataExportResultDto
{
    /// <summary>Gets the storage reference (blob URI) of the exported JSON artifact.</summary>
    public string ContentReference { get; init; } = string.Empty;

    /// <summary>Gets the size, in bytes, of the exported artifact.</summary>
    public long ContentSizeBytes { get; init; }

    /// <summary>Gets the UTC instant the export completed.</summary>
    public DateTime ExportedAtUtc { get; init; }

    /// <summary>Gets the region code the platform's current deployment region was validated against before this export was written.</summary>
    public string ValidatedRegionCode { get; init; } = string.Empty;
}

/// <summary>Request body for provisioning a new tenant.</summary>
public sealed class ProvisionTenantRequest
{
    /// <summary>Gets or sets the new tenant's display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the data-residency/deployment region the tenant should be pinned to.</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Gets or sets the subscription plan reference code.</summary>
    public string PlanCode { get; set; } = "Trial";

    /// <summary>Gets or sets the email address for the tenant's initial administrative user.</summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the first name for the tenant's initial administrative user.</summary>
    public string AdminFirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the last name for the tenant's initial administrative user.</summary>
    public string AdminLastName { get; set; } = string.Empty;
}

/// <summary>Request body for suspending a tenant.</summary>
public sealed class SuspendTenantRequest
{
    /// <summary>Gets or sets the human-readable reason for the suspension.</summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Request body for soft-deleting a tenant.</summary>
public sealed class DeleteTenantRequest
{
    /// <summary>Gets or sets the human-readable reason for the deletion.</summary>
    public string Reason { get; set; } = string.Empty;
}
