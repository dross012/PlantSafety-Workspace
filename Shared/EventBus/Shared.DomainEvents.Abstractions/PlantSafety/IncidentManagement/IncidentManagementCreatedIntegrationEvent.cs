// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.IncidentManagement
//  File:           IncidentManagementCreatedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.SafetyEvents.
//                  IncidentManagement.Events.IncidentManagementCreatedEvent (real domain event,
//                  raised by IncidentManagement.Create in PlantSafety.Domain). Additive-only
//                  evolution: new optional fields may be appended; existing fields are never
//                  removed or retyped within SchemaVersion 1.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;

/// <summary>Raised when a new IncidentManagement aggregate is created in PlantSafety.</summary>
/// <param name="IncidentManagementId">Identifier of the incident aggregate (GUID/UUIDv7).</param>
/// <param name="TenantId">Tenant that owns the incident.</param>
/// <param name="CreatedAtUtc">UTC instant the incident was created.</param>
/// <param name="ActorId">Identifier of the acting user, or null for system/AI/external actors.</param>
/// <param name="ActorKind">"User" | "System" | "AiAgent" | "External".</param>
/// <param name="CorrelationId">Correlation identifier inherited from the triggering command.</param>
/// <param name="CausationId">Causation identifier inherited from the triggering command.</param>
public sealed record IncidentManagementCreatedIntegrationEvent(
    Guid IncidentManagementId,
    string TenantId,
    DateTime CreatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Incidents.IncidentManagementCreated";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
