// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentCreatedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 (Path B: Transformative Modernization). Enriched
//                  V2 counterpart to AccidentCreatedIntegrationEvent (V1),
//                  adding FacilityId/IndustryName/Location/HazardType/
//                  TimestampUtc. Mirrors PlantSafety.Domain.AccidentManagement.
//                  Events.AccidentCreatedEvent -- the real domain event is
//                  UNMODIFIED; this V2 DTO is produced by
//                  CrossModuleDomainEventMapperV2 from that same, unchanged
//                  domain event, with enrichment resolved via
//                  IFacilityInferenceResolver/HazardInferenceRules (see
//                  Shared.EventBus.Abstractions.Contracts). The type name
//                  carries "V2" (rather than only bumping SchemaVersion, per
//                  IIntegrationEventContract.cs's usual convention)
//                  deliberately: V1 and V2 must coexist as distinct,
//                  independently-routable payload types while legacy
//                  subscribers keep consuming V1 unmodified.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when a new Accident aggregate is created in PlantSafety.</summary>
public sealed record AccidentCreatedIntegrationEventV2(
    Guid AccidentId,
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
    public static string EventType => "Accidents.AccidentCreatedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
