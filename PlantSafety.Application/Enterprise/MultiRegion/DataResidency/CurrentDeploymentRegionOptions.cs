// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           CurrentDeploymentRegionOptions.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Configuration for "which Region is THIS running deployment actually in" --
//                  honestly modeling this sandbox/platform's real single-region deployment
//                  reality (see docs\Phase7\multi-region-architecture.md's scope note: there are
//                  no real US East/US West/EU/APAC regions actually running anywhere). Defaults to
//                  Region.Default (US East) when unconfigured, matching that single-instance
//                  reality rather than pretending a multi-region topology exists. Bound from
//                  configuration section "MultiRegion:CurrentDeploymentRegion". Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>
/// Strongly typed configuration naming the single <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region"/>
/// code this running deployment is actually in.
/// </summary>
public sealed class CurrentDeploymentRegionOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "MultiRegion:CurrentDeploymentRegion";

    /// <summary>
    /// The <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region.Code"/> this running
    /// deployment is actually in. Null/empty falls back to
    /// <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region.Default"/> -- this
    /// platform's real, single-region deployment default.
    /// </summary>
    public string? RegionCode { get; init; }
}
