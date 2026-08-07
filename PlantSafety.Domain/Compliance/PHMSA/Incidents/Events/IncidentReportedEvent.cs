// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a new IncidentReport aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;

/// <summary>Raised when a new <see cref="Aggregates.IncidentReport"/> aggregate is created.</summary>
/// <remarks>
/// Includes <paramref name="IsReportableToPhmsa"/> so downstream consumers (e.g. a future
/// DOT 5800.1 submission workflow) can react to newly-created reports that already meet the
/// PHMSA reporting threshold without re-evaluating <see cref="Rules.IncidentReportabilityRules"/>.
/// </remarks>
public sealed record IncidentReportedEvent(
    IncidentReportEventId EventId,
    IncidentReportId IncidentReportId,
    IncidentType IncidentType,
    bool IsReportableToPhmsa,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentReported";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
