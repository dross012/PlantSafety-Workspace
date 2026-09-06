// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentCreatedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.AccidentManagement.
//                  Events.AccidentCreatedEvent (pre-existing domain event, raised by
//                  Accident.Create). Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Raised when a new Accident aggregate is created in PlantSafety.</summary>
public sealed record AccidentCreatedIntegrationEvent(
    Guid AccidentId,
    string TenantId,
    DateTime CreatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Accidents.AccidentCreated";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
