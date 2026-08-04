// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the RestrictedZonePolicy lifecycle: definition,
//                  activation, suspension, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Events;

/// <summary>Raised when a new restricted-zone policy is defined.</summary>
public sealed record RestrictedZonePolicyDefinedEvent(
    RestrictedZonePolicyEventId EventId,
    RestrictedZonePolicyId RestrictedZonePolicyId,
    Guid SafetyZoneId,
    string AllowedAccessLevel,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRestrictedZonePolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RestrictedZonePolicyDefined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a restricted-zone policy is activated and begins to be enforced.</summary>
public sealed record RestrictedZonePolicyActivatedEvent(
    RestrictedZonePolicyEventId EventId,
    RestrictedZonePolicyId RestrictedZonePolicyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRestrictedZonePolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RestrictedZonePolicyActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a restricted-zone policy is temporarily suspended.</summary>
public sealed record RestrictedZonePolicySuspendedEvent(
    RestrictedZonePolicyEventId EventId,
    RestrictedZonePolicyId RestrictedZonePolicyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRestrictedZonePolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RestrictedZonePolicySuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a restricted-zone policy is permanently retired.</summary>
public sealed record RestrictedZonePolicyRetiredEvent(
    RestrictedZonePolicyEventId EventId,
    RestrictedZonePolicyId RestrictedZonePolicyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRestrictedZonePolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RestrictedZonePolicyRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
