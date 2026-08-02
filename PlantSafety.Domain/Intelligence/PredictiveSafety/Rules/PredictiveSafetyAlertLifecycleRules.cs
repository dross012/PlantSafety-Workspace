// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Guards for the PredictiveSafetyAlert aggregate's lifecycle transitions. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Rules.RCALifecycleRules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.PredictiveSafety.Enums;

namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Rules;

/// <summary>Defines permitted lifecycle transitions for the PredictiveSafetyAlert aggregate.</summary>
public static class PredictiveSafetyAlertLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the alert may be acknowledged from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(PredictiveSafetyAlertStatus current) => current == PredictiveSafetyAlertStatus.Active;

    /// <summary>Returns <see langword="true"/> when the alert may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(PredictiveSafetyAlertStatus current) => current == PredictiveSafetyAlertStatus.Acknowledged;

    /// <summary>Returns <see langword="true"/> when the alert may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PredictiveSafetyAlertStatus current) => current == PredictiveSafetyAlertStatus.Resolved;
}
