// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PermitExpiration aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Events;

/// <summary>Raised when a new <see cref="PermitExpiration"/> aggregate is created.</summary>
public sealed record PermitExpirationCreatedEvent(
    PermitExpirationEventId EventId,
    PermitExpirationId      PermitExpirationId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPermitExpirationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitExpirationCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}