// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for HazardCommunicationTraining aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Policies;

/// <summary>Determines whether a <see cref="HazardCommunicationTraining"/> aggregate is eligible for archival.</summary>
public static class HazardCommunicationTrainingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the training record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining aggregate)
        => aggregate.Status == HazardCommunicationTrainingStatus.Closed;
}
