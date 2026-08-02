// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the RootCauseAnalysis aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Rules;

/// <summary>Defines permitted lifecycle transitions for the RootCauseAnalysis aggregate.</summary>
public static class RootCauseAnalysisLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the analysis may be submitted for review from <paramref name="current"/>.</summary>
    public static bool CanSubmitForReview(RootCauseAnalysisStatus current) => current == RootCauseAnalysisStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the analysis may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(RootCauseAnalysisStatus current) => current == RootCauseAnalysisStatus.PendingReview;

    /// <summary>Returns <see langword="true"/> when the analysis may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(RootCauseAnalysisStatus current) => current == RootCauseAnalysisStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the analysis may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(RootCauseAnalysisStatus current) => current == RootCauseAnalysisStatus.Closed;
}
