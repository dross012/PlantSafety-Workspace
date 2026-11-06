// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentCorrectiveActionCreatedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 DTO for PlantSafety.Domain.
//                  AccidentManagement.Events.AccidentCorrectiveActionCreatedEvent
//                  -- previously uncovered by any cross-module contract (see
//                  AccidentInvestigationClosedIntegrationEventV2.cs's header).
//                  Deliberately distinct from Shared.DomainEvents.Abstractions.
//                  PlantSafety.CAPA.CorrectiveActionCreatedIntegrationEvent
//                  (V1), which mirrors a DIFFERENT real domain event
//                  (PlantSafety.Domain.ContinuousImprovement.CAPA.Events.
//                  CorrectiveActionCreatedEvent) -- AccidentManagement has
//                  its own, separate CorrectiveAction-created fact.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when a new CAPA corrective action is created against an Accident.</summary>
public sealed record AccidentCorrectiveActionCreatedIntegrationEventV2(
    Guid AccidentId,
    string TenantId,
    Guid CorrectiveActionId,
    string Category,
    Guid ResponsiblePersonId,
    DateTime DueAtUtc,
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
    public static string EventType => "Accidents.AccidentCorrectiveActionCreatedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
