// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.ContinuousImprovement.CAPA
//  File:           CorrectiveActionClosedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring CorrectiveActionClosedEvent (verified,
//                  terminal state). Deliberately carries only fields backed by the domain event
//                  (CorrectiveActionId/ClosedAtUtc/actor context) -- no ClosedByActorId/
//                  ClosureNotes fields, since the domain event does not carry them either.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CAPA;

/// <summary>Raised when a CorrectiveAction is verified and formally closed.</summary>
public sealed record CorrectiveActionClosedIntegrationEvent(
    Guid CorrectiveActionId,
    string TenantId,
    DateTime ClosedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "CAPA.CorrectiveActionClosed";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
