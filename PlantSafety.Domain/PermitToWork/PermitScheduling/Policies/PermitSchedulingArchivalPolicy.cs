// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PermitScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Policies;

/// <summary>Determines whether a <see cref="PermitScheduling"/> aggregate is eligible for archival.</summary>
public static class PermitSchedulingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PermitScheduling may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling aggregate)
        => aggregate.Status == PermitSchedulingStatus.Closed;
}