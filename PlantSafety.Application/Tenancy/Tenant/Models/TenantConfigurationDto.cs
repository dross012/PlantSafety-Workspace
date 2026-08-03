// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read model for a Tenant's TenantConfiguration child entity (Phase 7 §2.2
//   Tenant Configuration): enabled optional modules, tenant-scoped feature-flag overrides, and
//   custom compliance rule references.
// SchemaVersion: 1

namespace PlantSafety.Application.Tenancy.Tenant.Models;

/// <summary>Read model for a Tenant's <c>TenantConfiguration</c>.</summary>
/// <param name="EnabledModules">Identifiers of optional platform modules enabled for this tenant.</param>
/// <param name="FeatureFlagOverrides">Tenant-scoped feature-flag overrides.</param>
/// <param name="CustomComplianceRuleCodes">Custom Compliance.RulesEngine rule codes this tenant has opted into.</param>
public sealed record TenantConfigurationDto(
    IReadOnlyList<string> EnabledModules,
    IReadOnlyList<TenantFeatureFlagOverrideDto> FeatureFlagOverrides,
    IReadOnlyList<string> CustomComplianceRuleCodes);

/// <summary>Read model for a single tenant-scoped feature-flag override.</summary>
/// <param name="FlagName">The overridden flag's identifier.</param>
/// <param name="IsEnabled">The tenant-specific enabled/disabled value.</param>
public sealed record TenantFeatureFlagOverrideDto(string FlagName, bool IsEnabled);
