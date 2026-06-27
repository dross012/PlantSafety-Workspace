// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           GovernanceCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a Governance aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Events;

/// <summary>Raised when a new <see cref="Governance"/> aggregate is created.</summary>
public sealed record GovernanceCreatedEvent(
    Guid          EventId,
    Guid          GovernanceId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GovernanceCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
