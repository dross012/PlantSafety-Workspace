// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           ICurrentDeploymentRegionProvider.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Abstraction over "which Region is THIS running deployment actually in" --
//                  the one piece of real, honest ground-truth this sandboxed single-region
//                  platform can offer for data-residency enforcement (see
//                  docs\Phase7\multi-region-architecture.md and
//                  CurrentDeploymentRegionOptions). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Resolves which <see cref="Region"/> this running deployment is actually in.</summary>
public interface ICurrentDeploymentRegionProvider
{
    /// <summary>The <see cref="Region"/> this running deployment is actually in.</summary>
    Region CurrentRegion { get; }
}
