// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with predicted-risk-score audit-trail
//                  field alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  PredictiveAnalytics aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="PredictiveAnalytics"/> aggregate.</summary>
public sealed class PredictiveAnalyticsHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent PredictiveAnalytics aggregate.</summary>
    public PredictiveAnalyticsId PredictiveAnalyticsId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Predicted risk score recorded at the time of this transition.</summary>
    public decimal PredictedRiskScoreAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private PredictiveAnalyticsHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public PredictiveAnalyticsHistoryEntry(
        Guid historyId, PredictiveAnalyticsId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal predictedRiskScoreAtTransition = 0m)
    {
        HistoryId                            = historyId;
        PredictiveAnalyticsId                = Id;
        PreviousStatus                        = previousStatus;
        NewStatus                             = newStatus;
        PredictedRiskScoreAtTransition         = predictedRiskScoreAtTransition;
        ChangedAtUtc                           = changedAtUtc;
        ChangedBy                              = changedBy;
        Notes                                  = notes;
    }
}
