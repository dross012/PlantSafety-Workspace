// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CultureAssessment aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Events;

/// <summary>Raised when a new <see cref="CultureAssessment"/> aggregate is created.</summary>
public sealed record CultureAssessmentCreatedEvent(
    CultureAssessmentEventId EventId,
    CultureAssessmentId      CultureAssessmentId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : ICultureAssessmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureAssessmentCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}