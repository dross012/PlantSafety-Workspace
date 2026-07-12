// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Training aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Policies;

/// <summary>Determines whether a <see cref="Training"/> aggregate is eligible for archival.</summary>
public static class TrainingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Training may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Operational.Training.Aggregates.Training aggregate)
        => aggregate.Status == TrainingStatus.Closed;
}
