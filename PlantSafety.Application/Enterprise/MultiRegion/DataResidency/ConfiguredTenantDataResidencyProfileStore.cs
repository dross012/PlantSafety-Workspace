// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           ConfiguredTenantDataResidencyProfileStore.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Default ITenantDataResidencyProfileStore, backed by TenantDataResidencyOptions.
//                  Pure, side-effect-free. Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Options;
using PlantSafety.Domain.MultiRegion.DataResidency;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Default <see cref="ITenantDataResidencyProfileStore"/>, backed by <see cref="TenantDataResidencyOptions"/>.</summary>
public sealed class ConfiguredTenantDataResidencyProfileStore : ITenantDataResidencyProfileStore
{
    private readonly TenantDataResidencyOptions _options;

    /// <summary>Initialises a new instance of <see cref="ConfiguredTenantDataResidencyProfileStore"/>.</summary>
    public ConfiguredTenantDataResidencyProfileStore(IOptions<TenantDataResidencyOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public TenantDataResidencyProfile GetProfile(Guid tenantId)
    {
        if (_options.Policies.TryGetValue(tenantId.ToString("D"), out var policyCode)
            && DataResidencyPolicy.TryParse(policyCode, out var policy))
        {
            return new TenantDataResidencyProfile(tenantId, policy!);
        }

        return TenantDataResidencyProfile.Unrestricted(tenantId);
    }
}
