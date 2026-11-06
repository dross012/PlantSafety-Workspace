// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.IncidentManagement
//  File:           IncidentManagementArchivedIntegrationEvent.cs
//  Created:        2026-07-27
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.SafetyEvents.
//                  IncidentManagement.Events.IncidentManagementArchivedEvent. Additive-only
//                  evolution. NOTE: there is deliberately NO "Closed" incident integration
//                  event -- no code path in PlantSafety.Domain closes an IncidentManagement
//                  aggregate today (see IncidentManagement.Aggregates for the confirmed
//                  transition set: Created -> Updated -> Archived only).
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;

/// <summary>Raised when an IncidentManagement aggregate is archived (soft-deleted) for long-term retention.</summary>
public sealed record IncidentManagementArchivedIntegrationEvent(
    Guid IncidentManagementId,
    string TenantId,
    DateTime ArchivedAtUtc,
    string DeletionReason,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Incidents.IncidentManagementArchived";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
