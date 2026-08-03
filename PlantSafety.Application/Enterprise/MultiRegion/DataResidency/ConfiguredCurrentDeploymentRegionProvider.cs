// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           ConfiguredCurrentDeploymentRegionProvider.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Default ICurrentDeploymentRegionProvider: reads CurrentDeploymentRegionOptions,
//                  falling back to Region.Default (US East) when unconfigured -- matching this
//                  platform's real single-region deployment reality. Pure, side-effect-free.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Options;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Default <see cref="ICurrentDeploymentRegionProvider"/>, backed by <see cref="CurrentDeploymentRegionOptions"/>.</summary>
public sealed class ConfiguredCurrentDeploymentRegionProvider : ICurrentDeploymentRegionProvider
{
    /// <summary>Initialises a new instance of <see cref="ConfiguredCurrentDeploymentRegionProvider"/>.</summary>
    public ConfiguredCurrentDeploymentRegionProvider(IOptions<CurrentDeploymentRegionOptions> options)
    {
        var code = options.Value.RegionCode;

        CurrentRegion = !string.IsNullOrWhiteSpace(code) && Region.TryParse(code, out var region)
            ? region!
            : Region.Default;
    }

    /// <inheritdoc/>
    public Region CurrentRegion { get; }
}
