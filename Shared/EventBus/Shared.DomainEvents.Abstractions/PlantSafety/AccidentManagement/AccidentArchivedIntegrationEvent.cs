// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentArchivedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.AccidentManagement.
//                  Events.AccidentArchivedEvent (pre-existing domain event, raised when a Closed
//                  Accident is archived for long-term retention). Additive-only evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Raised when a Closed Accident is archived for long-term retention.</summary>
public sealed record AccidentArchivedIntegrationEvent(
    Guid AccidentId,
    string TenantId,
    DateTime ArchivedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Accidents.AccidentArchived";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
