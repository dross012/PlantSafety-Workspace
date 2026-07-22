// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentReportPublishedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a finalized CultureAssessment report is published to
//                  Scorecards and executive dashboards (PlantSafety Program Structure,
//                  "Safety Culture Module" §10, "These metrics feed directly into Scorecards
//                  and executive dashboards").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Events;

/// <summary>Raised when a <see cref="CultureAssessment"/> report is published.</summary>
public sealed record CultureAssessmentReportPublishedEvent(
    CultureAssessmentEventId EventId,
    CultureAssessmentId CultureAssessmentId,
    decimal? OverallCultureScore,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureAssessmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureAssessmentReportPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
