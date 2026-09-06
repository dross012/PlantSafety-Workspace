// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentRegulatoryFilingCompletedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 DTO for PlantSafety.Domain.
//                  AccidentManagement.Events.AccidentRegulatoryFilingCompletedEvent
//                  -- previously uncovered by any cross-module contract (see
//                  AccidentInvestigationClosedIntegrationEventV2.cs's header).
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when required regulatory documentation (e.g. OSHA 300/300A/301) is filed for an Accident.</summary>
public sealed record AccidentRegulatoryFilingCompletedIntegrationEventV2(
    Guid AccidentId,
    string TenantId,
    Guid FiledBy,
    string RegulatoryAgency,
    string ReferenceNumber,
    DateTime FiledAtUtc,
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
    public static string EventType => "Accidents.AccidentRegulatoryFilingCompletedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
