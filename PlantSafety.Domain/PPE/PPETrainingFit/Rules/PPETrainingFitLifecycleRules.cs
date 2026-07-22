// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PPETrainingFit aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PPETrainingFit"/> aggregate.</summary>
public static class PPETrainingFitLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PPETrainingFitStatus current) => current == PPETrainingFitStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PPETrainingFitStatus current) => current == PPETrainingFitStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PPETrainingFitStatus current) => current == PPETrainingFitStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PPETrainingFitStatus current) => current == PPETrainingFitStatus.Closed;
}