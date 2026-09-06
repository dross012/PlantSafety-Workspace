// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.ContinuousImprovement.CAPA
//  File:           CorrectiveActionCreatedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.ContinuousImprovement.
//                  CAPA.Events.CorrectiveActionCreatedEvent. Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CAPA;

/// <summary>Raised when a new CAPA CorrectiveAction item is created in PlantSafety.</summary>
public sealed record CorrectiveActionCreatedIntegrationEvent(
    Guid CorrectiveActionId,
    string TenantId,
    Guid SourceEventId,
    string SourceEventType,
    Guid AssignedToWorkerId,
    DateTime DueAtUtc,
    DateTime CreatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "CAPA.CorrectiveActionCreated";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
