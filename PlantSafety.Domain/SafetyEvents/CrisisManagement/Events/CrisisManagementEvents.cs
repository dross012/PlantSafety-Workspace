// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the CrisisManagement aggregate's full lifecycle
//                  (Declare/RespondTo/Stabilize/Resolve/Close/Archive). Phase 4 full build-out --
//                  this aggregate did not exist before this pass (only a placeholder marker class
//                  and a read-only list query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;

/// <summary>Raised when a new crisis is declared.</summary>
public sealed record CrisisManagementDeclaredEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    Guid TenantId,
    Guid? SiteId,
    string Title,
    string Severity,
    UtcTimestamp DeclaredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementDeclared";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeclaredAtUtc;
}

/// <summary>Raised when a responder is assigned/responds to a declared crisis (Declared -&gt; Active).</summary>
public sealed record CrisisManagementRespondedToEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    UserId RespondedBy,
    UtcTimestamp RespondedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementRespondedTo";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RespondedAtUtc;
}

/// <summary>Raised when an active crisis is reported stabilized.</summary>
public sealed record CrisisManagementStabilizedEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    UserId StabilizedBy,
    UtcTimestamp StabilizedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementStabilized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StabilizedAtUtc;
}

/// <summary>Raised when an active crisis is resolved (Active -&gt; Completed).</summary>
public sealed record CrisisManagementResolvedEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    UserId ResolvedBy,
    UtcTimestamp ResolvedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}

/// <summary>Raised when a resolved crisis is formally closed (Completed -&gt; Closed).</summary>
public sealed record CrisisManagementClosedEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed crisis record is archived (Closed -&gt; Archived).</summary>
public sealed record CrisisManagementArchivedEvent(
    CrisisManagementEventId EventId,
    CrisisManagementId CrisisManagementId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICrisisManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrisisManagementArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
