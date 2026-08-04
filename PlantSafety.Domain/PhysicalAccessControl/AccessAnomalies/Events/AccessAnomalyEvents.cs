// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AccessAnomaly lifecycle: type-specific
//                  detection events (feeding RiskManagement and AlertEscalationPolicy) plus
//                  acknowledgement, escalation, and resolution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Events;

/// <summary>Raised when an unrecognized, suspended, or revoked credential is detected.</summary>
public sealed record UnauthorizedAccessDetectedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    Guid AccessPointId,
    Guid? CredentialId,
    decimal SeverityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "UnauthorizedAccessDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a tailgating condition is detected at an access point.</summary>
public sealed record TailgatingDetectedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    Guid AccessPointId,
    Guid? CredentialId,
    decimal SeverityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TailgatingDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when access occurs outside a zone's/policy's approved hours.</summary>
public sealed record AfterHoursAccessDetectedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    Guid AccessPointId,
    Guid? CredentialId,
    decimal SeverityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AfterHoursAccessDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a credential without the required access level is used against a restricted zone.</summary>
public sealed record RestrictedZoneViolationEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    Guid AccessPointId,
    Guid? CredentialId,
    Guid SafetyZoneId,
    decimal SeverityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RestrictedZoneViolation";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an anomaly is acknowledged by a responder.</summary>
public sealed record AccessAnomalyAcknowledgedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    Guid AcknowledgedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessAnomalyAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an anomaly is escalated (e.g. via AlertEscalationPolicy).</summary>
public sealed record AccessAnomalyEscalatedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessAnomalyEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an anomaly investigation is resolved.</summary>
public sealed record AccessAnomalyResolvedEvent(
    AccessAnomalyEventId EventId,
    AccessAnomalyId AccessAnomalyId,
    string Resolution,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessAnomalyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessAnomalyResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
