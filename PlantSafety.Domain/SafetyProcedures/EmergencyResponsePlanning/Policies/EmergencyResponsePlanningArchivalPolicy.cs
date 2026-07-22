// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for EmergencyResponsePlanning
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Policies;

/// <summary>Determines whether a <see cref="EmergencyResponsePlanning"/> aggregate is eligible for archival.</summary>
public static class EmergencyResponsePlanningArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the plan document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning aggregate)
        => aggregate.Status == EmergencyResponsePlanningStatus.Retired;
}
