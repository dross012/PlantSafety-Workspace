// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.RiskManagement
//  File:           RiskCreatedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.RiskManagement.
//                  Events.RiskCreatedEvent (pre-existing domain event, raised by Risk.Create).
//                  Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.RiskManagement;

/// <summary>Raised when a new Risk aggregate is created in PlantSafety.</summary>
public sealed record RiskCreatedIntegrationEvent(
    Guid RiskId,
    string TenantId,
    DateTime CreatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Risk.RiskCreated";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
