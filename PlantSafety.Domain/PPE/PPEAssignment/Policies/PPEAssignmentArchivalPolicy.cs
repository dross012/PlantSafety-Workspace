// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPEAssignment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Policies;

/// <summary>Determines whether a <see cref="PPEAssignment"/> aggregate is eligible for archival.</summary>
public static class PPEAssignmentArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPEAssignment may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment aggregate)
        => aggregate.Status == PPEAssignmentStatus.Closed;
}