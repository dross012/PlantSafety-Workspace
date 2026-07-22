// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for HazardCommunicationRightToKnow
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Policies;

/// <summary>Determines whether a <see cref="HazardCommunicationRightToKnow"/> aggregate is eligible for archival.</summary>
public static class HazardCommunicationRightToKnowArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the procedure document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow aggregate)
        => aggregate.Status == HazardCommunicationRightToKnowStatus.Retired;
}
