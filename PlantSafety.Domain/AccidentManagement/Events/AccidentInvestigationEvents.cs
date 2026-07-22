// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentInvestigationEvents.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real Accident investigation lifecycle:
//                  investigator assignment, root-cause recording, CAPA creation/completion/
//                  verification, regulatory filing, and closure. Grounded in the Accident
//                  Management section of PlantSafety_Program_Structure_extract.txt.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Events;

/// <summary>Raised when an investigator is assigned to an Accident.</summary>
public sealed record AccidentInvestigatorAssignedEvent(
    AccidentEventId  EventId,
    AccidentId       AccidentId,
    UserId           InvestigatorId,
    UtcTimestamp     AssignedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentInvestigatorAssigned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssignedAtUtc;
}

/// <summary>Raised when Root Cause Analysis completes and a root cause is recorded.</summary>
public sealed record AccidentRootCauseRecordedEvent(
    AccidentEventId  EventId,
    AccidentId       AccidentId,
    RootCauseCategory RootCause,
    UtcTimestamp     RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentRootCauseRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}

/// <summary>Raised when a new CAPA corrective action is created against an Accident.</summary>
public sealed record AccidentCorrectiveActionCreatedEvent(
    AccidentEventId       EventId,
    AccidentId            AccidentId,
    CorrectiveActionId    CorrectiveActionId,
    CorrectiveActionCategory Category,
    UserId                ResponsiblePersonId,
    UtcTimestamp          DueAtUtc,
    UtcTimestamp          CreatedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentCorrectiveActionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

/// <summary>Raised when a CAPA corrective action is marked Completed.</summary>
public sealed record AccidentCorrectiveActionCompletedEvent(
    AccidentEventId    EventId,
    AccidentId         AccidentId,
    CorrectiveActionId CorrectiveActionId,
    UtcTimestamp       CompletedAtUtc,
    PerformedByActor   Actor,
    CorrelationId      CorrelationId,
    CausationId        CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentCorrectiveActionCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when required regulatory documentation (e.g. OSHA 300/300A/301) is filed.</summary>
public sealed record AccidentRegulatoryFilingCompletedEvent(
    AccidentEventId  EventId,
    AccidentId       AccidentId,
    UserId           FiledBy,
    string           RegulatoryAgency,
    string           ReferenceNumber,
    UtcTimestamp     FiledAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentRegulatoryFilingCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FiledAtUtc;
}

/// <summary>Raised when an Accident investigation is closed.</summary>
public sealed record AccidentInvestigationClosedEvent(
    AccidentEventId  EventId,
    AccidentId       AccidentId,
    UserId           ClosedBy,
    UtcTimestamp     ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentInvestigationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
