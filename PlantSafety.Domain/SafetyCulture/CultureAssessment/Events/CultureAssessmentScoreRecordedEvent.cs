// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentScoreRecordedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when the overall culture health score and leadership engagement
//                  score are (re)computed for a CultureAssessment period. Feeds the Executive
//                  Safety Overview Dashboard's "Safety Culture Score Gauge" (PlantSafety
//                  Program Structure, "FULL PROGRAM DASHBOARD LIST" §1).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Events;

/// <summary>Raised when culture scores are recorded on a <see cref="CultureAssessment"/>.</summary>
public sealed record CultureAssessmentScoreRecordedEvent(
    CultureAssessmentEventId EventId,
    CultureAssessmentId CultureAssessmentId,
    decimal OverallCultureScore,
    decimal? LeadershipEngagementScore,
    UtcTimestamp RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureAssessmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureAssessmentScoreRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
