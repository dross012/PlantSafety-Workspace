// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the EmergencyManagement aggregate's response lifecycle
//                  (RespondTo/Stabilize/Resolve/Close/Archive), raised alongside the pre-existing
//                  EmergencyManagementCreatedEvent. Phase 4 full build-out -- this aggregate
//                  previously had only a Create factory (StabilizedAtUtc/ResolvedAtUtc were
//                  already-scaffolded properties nothing ever set).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;

/// <summary>Raised when the response team begins responding to a declared emergency (Created -&gt; Active).</summary>
public sealed record EmergencyManagementRespondedToEvent(
    EmergencyManagementEventId EventId,
    EmergencyManagementId EmergencyManagementId,
    UserId RespondedBy,
    UtcTimestamp RespondedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyManagementRespondedTo";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RespondedAtUtc;
}

/// <summary>Raised when the emergency is stabilized during active response. Does not change lifecycle status.</summary>
public sealed record EmergencyManagementStabilizedEvent(
    EmergencyManagementEventId EventId,
    EmergencyManagementId EmergencyManagementId,
    UserId StabilizedBy,
    UtcTimestamp StabilizedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyManagementStabilized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StabilizedAtUtc;
}

/// <summary>Raised when the emergency is resolved (Active -&gt; Completed).</summary>
public sealed record EmergencyManagementResolvedEvent(
    EmergencyManagementEventId EventId,
    EmergencyManagementId EmergencyManagementId,
    UserId ResolvedBy,
    UtcTimestamp ResolvedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyManagementResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}

/// <summary>Raised when a resolved emergency record is closed (Completed -&gt; Closed).</summary>
public sealed record EmergencyManagementClosedEvent(
    EmergencyManagementEventId EventId,
    EmergencyManagementId EmergencyManagementId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyManagementClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed emergency record is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record EmergencyManagementArchivedEvent(
    EmergencyManagementEventId EventId,
    EmergencyManagementId EmergencyManagementId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyManagementArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
