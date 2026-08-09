// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           TenantFacilityMap.cs
//  Created:        2026-08-08
//  Description:    Phase C.8 (Path C: Operational Modernization): the
//                  multi-facility successor to TenantFacilityRegistration --
//                  a tenant can now be bound to many facilities, with one
//                  designated primary (used for default dashboard routing).
//                  Additive: TenantFacilityRegistration/ITenantFacilityRegistry
//                  (single-facility-per-tenant, Phase 3) are untouched;
//                  ITenantFacilityRegistryV2 is a new, parallel registry.
//                  Facility ids are kept as plain Guid, not Shared.
//                  FacilityDomain.FacilityId, matching this project's
//                  standing no-cross-repo-type-dependency rule (see
//                  Shared.EventBus's EventEnvelopeV2 precedent).
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>Maps a PlantSafety <c>TenantId</c> to every facility it is bound to, with one designated primary.</summary>
/// <param name="TenantId">The PlantSafety tenant identifier.</param>
/// <param name="FacilityIds">Every facility id this tenant is bound to, in registration order.</param>
/// <param name="PrimaryFacilityId">The facility id used for default dashboard routing, or null if the tenant has no facilities.</param>
public sealed record TenantFacilityMap(
    string TenantId,
    IReadOnlyList<Guid> FacilityIds,
    Guid? PrimaryFacilityId);
