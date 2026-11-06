// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.IncidentManagement
//  File:           IncidentManagementUpdatedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.SafetyEvents.
//                  IncidentManagement.Events.IncidentManagementUpdatedEvent. Additive-only
//                  evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;

/// <summary>Raised when an existing IncidentManagement aggregate's mutable details are updated.</summary>
public sealed record IncidentManagementUpdatedIntegrationEvent(
    Guid IncidentManagementId,
    string TenantId,
    DateTime UpdatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Incidents.IncidentManagementUpdated";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
