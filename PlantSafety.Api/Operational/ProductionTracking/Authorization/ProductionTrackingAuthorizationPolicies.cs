// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Api
//  File:           ProductionTrackingAuthorizationPolicies.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Authorization policy name constants for the Operational.ProductionTracking
//                  module. Auto-discovered by AuthorizationPolicyExtensions.AddPlantSafetyPolicies()
//                  (reflects over every "*AuthorizationPolicies" class in the PlantSafety.Api
//                  assembly) -- no other file needs to change for these policies to be registered.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Api.Operational.ProductionTracking.Authorization;

/// <summary>Authorization policy name constants for the Operational.ProductionTracking module.</summary>
public static class ProductionTrackingAuthorizationPolicies
{
    /// <summary>Read access to ProductionLine/ProductionRun data. Granted to authorized roles.</summary>
    public const string Read = "RequireOperationalProductionTrackingRead";

    /// <summary>Write access to ProductionLine/ProductionRun data. Granted to administrative/operational roles.</summary>
    public const string Write = "RequireOperationalProductionTrackingWrite";
}
