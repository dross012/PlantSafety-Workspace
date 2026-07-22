// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for PermitToWork aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Policies;

/// <summary>Determines whether a <see cref="PermitToWork"/> aggregate is eligible for archival.</summary>
public static class PermitToWorkArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the permit record may be archived (closed or expired).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork aggregate)
        => aggregate.Status is PermitToWorkStatus.Closed or PermitToWorkStatus.Expired;
}
