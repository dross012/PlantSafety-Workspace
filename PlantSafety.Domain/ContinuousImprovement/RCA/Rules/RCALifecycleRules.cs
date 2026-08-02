// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCALifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Guards for the RCA aggregate's lifecycle transitions. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Rules
//                  .RootCauseAnalysisLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.RCA.Enums;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Rules;

/// <summary>Defines permitted lifecycle transitions for the RCA aggregate.</summary>
public static class RCALifecycleRules
{
    /// <summary>Returns <see langword="true"/> when analysis may be started from <paramref name="current"/>.</summary>
    public static bool CanStartAnalysis(RCAStatus current) => current == RCAStatus.Initiated;

    /// <summary>Returns <see langword="true"/> when the RCA may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(RCAStatus current) => current == RCAStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the RCA may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(RCAStatus current) => current == RCAStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when the RCA may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(RCAStatus current) => current == RCAStatus.Closed;
}
