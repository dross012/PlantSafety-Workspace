// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Api
//  File:           ErpIntegrationAuthorizationPolicies.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Authorization policy name constants for the Enterprise.Integrations.Erp
//                  sub-feature. Auto-discovered and registered by
//                  PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//  Rationale:      A Phase 6 completion-validation pass found this controller enforcing only
//                  [Authorize] (any authenticated user, any tier) on operations that trigger an
//                  outbound ERP asset sync and read/write tenant cost-center mappings -- both
//                  financially/operationally consequential cross-system actions. Per explicit
//                  product decision, gated at Approver tier (matching this codebase's convention
//                  for other cross-system/compliance-sensitive writes, e.g. AuditReport
//                  generation), for all three endpoints -- read included, since a cost-center
//                  mapping is reference data that feeds financial posting hooks.
// ============================================================================

namespace PlantSafety.Api.Enterprise.Integrations.Erp.Authorization;

/// <summary>Authorization policy name constants for the Enterprise.Integrations.Erp sub-feature.</summary>
public static class ErpIntegrationAuthorizationPolicies
{
    /// <summary>
    /// Access to trigger an ERP asset-master sync and to read/write tenant cost-center mappings.
    /// Granted to approver-and-above roles: these are cross-system, financially-consequential
    /// operations, not ordinary domain reads/writes.
    /// </summary>
    public const string Approve = "RequireEnterpriseIntegrationsErpApprove";
}
