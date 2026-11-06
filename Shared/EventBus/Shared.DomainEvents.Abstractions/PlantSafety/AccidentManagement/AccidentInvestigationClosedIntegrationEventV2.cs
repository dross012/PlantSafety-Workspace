// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentInvestigationClosedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 DTO for PlantSafety.Domain.
//                  AccidentManagement.Events.AccidentInvestigationClosedEvent
//                  -- a real domain event that, before this phase, had NO
//                  cross-module integration-event contract at all (see
//                  CrossModuleDomainEventMapper.cs's header: only Accident
//                  Created/Archived were curated for V1). This is the first
//                  cross-module contract for this fact. See
//                  AccidentCreatedIntegrationEventV2.cs's header for the
//                  general V2 design rationale.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when an Accident investigation is closed.</summary>
public sealed record AccidentInvestigationClosedIntegrationEventV2(
    Guid AccidentId,
    string TenantId,
    Guid ClosedBy,
    DateTime ClosedAtUtc,
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
    public static string EventType => "Accidents.AccidentInvestigationClosedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
