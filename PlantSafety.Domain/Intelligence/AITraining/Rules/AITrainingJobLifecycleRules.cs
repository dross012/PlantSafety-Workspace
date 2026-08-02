// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Guards for the AITrainingJob aggregate's lifecycle transitions. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Rules.RCALifecycleRules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.AITraining.Enums;

namespace PlantSafety.Domain.Intelligence.AITraining.Rules;

/// <summary>Defines permitted lifecycle transitions for the AITrainingJob aggregate.</summary>
public static class AITrainingJobLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the training job may be started from <paramref name="current"/>.</summary>
    public static bool CanStart(AITrainingJobStatus current) => current == AITrainingJobStatus.Queued;

    /// <summary>Returns <see langword="true"/> when the training job may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(AITrainingJobStatus current) => current == AITrainingJobStatus.Running;

    /// <summary>Returns <see langword="true"/> when the training job may be marked failed from <paramref name="current"/>.</summary>
    public static bool CanFail(AITrainingJobStatus current) => current == AITrainingJobStatus.Running;

    /// <summary>Returns <see langword="true"/> when the training job may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AITrainingJobStatus current) =>
        current is AITrainingJobStatus.Completed or AITrainingJobStatus.Failed;
}
