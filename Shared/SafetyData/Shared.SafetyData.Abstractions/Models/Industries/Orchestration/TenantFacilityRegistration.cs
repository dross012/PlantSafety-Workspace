// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           TenantFacilityRegistration.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Interim correlation record mapping a PlantSafety TenantId
//                  (the only facility-adjacent identifier real domain events
//                  carry today -- see EventOrchestrationService.cs's header
//                  for the full investigation trail) to the identity/industry
//                  binding FacilityProfileBuilder.Build needs. This exists
//                  because PlantSafety has no persisted Facility aggregate
//                  and no domain event anywhere carries facilityId/
//                  industryName/location -- see Facilities\FacilityProfile.cs's
//                  own SCOPE NOTE. A real facility registry (if/when
//                  PlantSafety builds one) should replace ITenantFacilityRegistry's
//                  in-memory implementation, not this record shape.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>Maps a PlantSafety <c>TenantId</c> to the facility identity/industry binding needed to build a <see cref="Facilities.FacilityProfile"/>.</summary>
/// <param name="TenantId">The PlantSafety tenant identifier (matches <c>Shared.EventBus.Abstractions.Contracts.EventEnvelope.TenantId</c>'s string shape).</param>
/// <param name="FacilityId">The facility identifier to build profiles with.</param>
/// <param name="Name">The facility's display name.</param>
/// <param name="IndustryName">The exact, canonical industry name this facility is bound to.</param>
/// <param name="Location">The facility's location (free text).</param>
public sealed record TenantFacilityRegistration(
    string TenantId,
    Guid FacilityId,
    string Name,
    string IndustryName,
    string Location);
