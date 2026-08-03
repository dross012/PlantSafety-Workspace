// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Maps the Tenant aggregate to its canonical TenantDto read model.
// SchemaVersion: 1

using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Mapping;

/// <summary>Maps <see cref="Domain.Tenancy.Tenant.Aggregates.Tenant"/> to <see cref="TenantDto"/>.</summary>
public static class TenantMapper
{
    /// <summary>Maps a Tenant aggregate to its DTO.</summary>
    public static TenantDto ToDto(Domain.Tenancy.Tenant.Aggregates.Tenant tenant) => new(
        TenantId: tenant.TenantId.Value,
        Name: tenant.Name,
        Region: tenant.Region,
        Status: tenant.Status.Code,
        PlanCode: tenant.PlanCode.Code,
        Configuration: new TenantConfigurationDto(
            EnabledModules: tenant.Configuration.EnabledModules,
            FeatureFlagOverrides: tenant.Configuration.FeatureFlagOverrides
                .Select(f => new TenantFeatureFlagOverrideDto(f.FlagName, f.IsEnabled))
                .ToList(),
            CustomComplianceRuleCodes: tenant.Configuration.CustomComplianceRuleCodes),
        CreatedAtUtc: tenant.CreatedAtUtc.Value,
        ActivatedAtUtc: tenant.ActivatedAtUtc?.Value,
        SuspendedAtUtc: tenant.SuspendedAtUtc?.Value,
        SuspensionReason: tenant.SuspensionReason,
        ArchivedAtUtc: tenant.ArchivedAtUtc?.Value,
        DeletedAtUtc: tenant.DeletedAtUtc?.Value);
}
