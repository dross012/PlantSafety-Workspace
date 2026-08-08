// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           ITenantFacilityRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Interim registry resolving a TenantId to a
//                  TenantFacilityRegistration -- see that file's header for
//                  why this exists. Register is expected to be called once
//                  per tenant by whatever future onboarding flow captures a
//                  tenant's facility identity/industry/location; TryResolve
//                  is called by EventOrchestrationService for every inbound
//                  event.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>Resolves a PlantSafety TenantId to its <see cref="TenantFacilityRegistration"/>.</summary>
public interface ITenantFacilityRegistry
{
    /// <summary>Registers (or replaces) the facility binding for the given tenant.</summary>
    void Register(TenantFacilityRegistration registration);

    /// <summary>Attempts to resolve the facility binding for the given tenant.</summary>
    bool TryResolve(string tenantId, out TenantFacilityRegistration? registration);
}
