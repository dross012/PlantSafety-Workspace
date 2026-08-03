// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Api
//  File:           WorkOrdersAuthorizationPolicies.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Authorization policy name constants for the Operational.WorkOrders module.
//                  Auto-discovered by AuthorizationPolicyExtensions.AddPlantSafetyPolicies() (reflects
//                  over every "*AuthorizationPolicies" class in the PlantSafety.Api assembly) -- no
//                  other file needs to change for these policies to be registered. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Api.Operational.WorkOrders.Authorization;

/// <summary>Authorization policy name constants for the Operational.WorkOrders module.</summary>
public static class WorkOrdersAuthorizationPolicies
{
    /// <summary>Read access to WorkOrder/Part data. Granted to authorized roles.</summary>
    public const string Read = "RequireOperationalWorkOrdersRead";

    /// <summary>Write access to WorkOrder/Part data. Granted to administrative roles.</summary>
    public const string Write = "RequireOperationalWorkOrdersWrite";
}
