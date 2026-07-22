// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPETrainingFit aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Policies;

/// <summary>Determines whether a <see cref="PPETrainingFit"/> aggregate is eligible for archival.</summary>
public static class PPETrainingFitArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPETrainingFit may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit aggregate)
        => aggregate.Status == PPETrainingFitStatus.Closed;
}