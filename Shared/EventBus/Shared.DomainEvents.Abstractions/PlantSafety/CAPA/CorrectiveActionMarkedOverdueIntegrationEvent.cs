// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.ContinuousImprovement.CAPA
//  File:           CorrectiveActionMarkedOverdueIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring CorrectiveActionMarkedOverdueEvent.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CAPA;

/// <summary>Raised when a CorrectiveAction passes its due date without completion.</summary>
public sealed record CorrectiveActionMarkedOverdueIntegrationEvent(
    Guid CorrectiveActionId,
    string TenantId,
    DateTime MarkedOverdueAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "CAPA.CorrectiveActionMarkedOverdue";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
