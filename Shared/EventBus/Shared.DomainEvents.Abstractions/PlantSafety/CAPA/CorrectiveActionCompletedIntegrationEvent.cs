// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.ContinuousImprovement.CAPA
//  File:           CorrectiveActionCompletedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring CorrectiveActionCompletedEvent
//                  (work complete, awaiting verification). Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CAPA;

/// <summary>Raised when a CorrectiveAction's work is completed (awaiting verification).</summary>
public sealed record CorrectiveActionCompletedIntegrationEvent(
    Guid CorrectiveActionId,
    string TenantId,
    DateTime CompletedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "CAPA.CorrectiveActionCompleted";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
