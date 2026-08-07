// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportLifecycleEvents.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain events for the IncidentReport aggregate's Submit/BeginReview/Close
//                  lifecycle, raised alongside the pre-existing IncidentReportedEvent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;

/// <summary>Raised when a report is submitted for review (Draft -&gt; Submitted).</summary>
public sealed record IncidentReportSubmittedEvent(
    IncidentReportEventId EventId,
    IncidentReportId IncidentReportId,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentReportSubmitted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when review begins on a submitted report (Submitted -&gt; UnderReview).</summary>
public sealed record IncidentReportReviewStartedEvent(
    IncidentReportEventId EventId,
    IncidentReportId IncidentReportId,
    UtcTimestamp ReviewStartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentReportReviewStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewStartedAtUtc;
}

/// <summary>Raised when a report under review is formally closed (UnderReview -&gt; Closed).</summary>
public sealed record IncidentReportClosedEvent(
    IncidentReportEventId EventId,
    IncidentReportId IncidentReportId,
    string? ResolutionNotes,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentReportClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
