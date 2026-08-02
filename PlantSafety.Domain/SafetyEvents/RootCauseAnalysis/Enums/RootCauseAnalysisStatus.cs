// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the RootCauseAnalysis aggregate root. Matches the
//                  vocabulary already documented on the pre-existing read-model DTO's Status
//                  property (e.g., InProgress/PendingReview/Completed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Enums;

/// <summary>Lifecycle states for the RootCauseAnalysis aggregate root.</summary>
public enum RootCauseAnalysisStatus
{
    /// <summary>The analysis is underway.</summary>
    InProgress = 0,

    /// <summary>The analysis has been submitted and awaits review/approval.</summary>
    PendingReview = 1,

    /// <summary>The analysis has been reviewed and approved.</summary>
    Completed = 2,

    /// <summary>The analysis has been formally closed.</summary>
    Closed = 3,

    /// <summary>The analysis has been archived for compliance retention.</summary>
    Archived = 4,
}
