// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for EmergencyPreparedness aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Policies;

/// <summary>Determines whether a <see cref="EmergencyPreparedness"/> aggregate is eligible for archival.</summary>
public static class EmergencyPreparednessArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the plan record may be archived (readiness verified only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness aggregate)
        => aggregate.Status == EmergencyPreparednessStatus.ReadinessVerified;
}
