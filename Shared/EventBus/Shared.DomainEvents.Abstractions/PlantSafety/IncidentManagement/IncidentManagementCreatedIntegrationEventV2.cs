// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.IncidentManagement
//  File:           IncidentManagementCreatedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 counterpart to
//                  IncidentManagementCreatedIntegrationEvent (V1). This is
//                  what the phase's own spec named "SafetyEventOccurredEventV2"
//                  ("SafetyEvents domain") maps onto in reality: no domain
//                  event literally named "SafetyEventOccurred" exists (only
//                  a marker interface, PlantSafety.Domain.SafetyEvents.Events.
//                  ISafetyEventDomainEvent, with no concrete implementation
//                  at the SafetyEvents root -- verified before writing this
//                  file). IncidentManagement is the real, concrete "a safety
//                  event occurred" fact under the SafetyEvents domain, and
//                  is the only one with any prior cross-module integration
//                  contract at all. See AccidentCreatedIntegrationEventV2.cs's
//                  header for the general V2 design rationale.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;

/// <summary>Enriched V2: raised when a new IncidentManagement aggregate is created in PlantSafety.</summary>
public sealed record IncidentManagementCreatedIntegrationEventV2(
    Guid IncidentManagementId,
    string TenantId,
    DateTime CreatedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId,
    Guid? FacilityId,
    string? IndustryName,
    string? Location,
    string? HazardType,
    DateTime TimestampUtc
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Incidents.IncidentManagementCreatedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
