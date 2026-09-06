// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.RiskManagement
//  File:           RiskArchivedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.RiskManagement.
//                  Events.RiskArchivedEvent (pre-existing domain event, raised when a Closed
//                  Risk is archived for long-term retention). Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.RiskManagement;

/// <summary>Raised when a Closed Risk is archived for long-term retention.</summary>
public sealed record RiskArchivedIntegrationEvent(
    Guid RiskId,
    string TenantId,
    DateTime ArchivedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Risk.RiskArchived";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
