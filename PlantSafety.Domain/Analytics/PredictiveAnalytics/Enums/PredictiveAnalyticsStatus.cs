// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsStatus.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Enums;

/// <summary>Lifecycle states for the <see cref="PredictiveAnalytics"/> aggregate root.</summary>
public enum PredictiveAnalyticsStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}
