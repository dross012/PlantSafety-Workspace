// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.ContinuousImprovement.CAPA
//  File:           CorrectiveActionStartedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring CorrectiveActionStartedEvent
//                  (Open -> InProgress). Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CAPA;

/// <summary>Raised when a CorrectiveAction transitions from Open to InProgress.</summary>
public sealed record CorrectiveActionStartedIntegrationEvent(
    Guid CorrectiveActionId,
    string TenantId,
    DateTime StartedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "CAPA.CorrectiveActionStarted";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
