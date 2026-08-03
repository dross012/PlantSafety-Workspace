// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantLifecycleRules.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.3 Tenant Lifecycle Management)
//  SchemaVersion:  2026.1
//  Description:    Explicit finite state machine transition rules for the Tenant aggregate:
//                  Provisioning -> Active -> Suspended -> Active | Archived -> Deleted (soft
//                  delete, terminal). Mirrors the platform's established
//                  OptimizationRunLifecycleRules/WorkflowInstance precedent. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Tenancy.Tenant.Enums;

namespace PlantSafety.Domain.Tenancy.Tenant.Rules;

/// <summary>Guards every Tenant lifecycle state transition.</summary>
public static class TenantLifecycleRules
{
    /// <summary>Provisioning may complete into Active.</summary>
    public static bool CanActivate(TenantStatus current) => current.Equals(TenantStatus.Provisioning);

    /// <summary>Only an Active tenant may be suspended.</summary>
    public static bool CanSuspend(TenantStatus current) => current.Equals(TenantStatus.Active);

    /// <summary>Only a Suspended tenant may be reactivated back to Active.</summary>
    public static bool CanReactivate(TenantStatus current) => current.Equals(TenantStatus.Suspended);

    /// <summary>Active or Suspended tenants may have their data archived.</summary>
    public static bool CanArchive(TenantStatus current) =>
        current.Equals(TenantStatus.Active) || current.Equals(TenantStatus.Suspended);

    /// <summary>Any non-terminal status may be soft-deleted (Deleted is terminal; a tenant already Deleted cannot be deleted again).</summary>
    public static bool CanDelete(TenantStatus current) => !current.Equals(TenantStatus.Deleted);

    /// <summary>Configuration may be updated for any status except a terminal Deleted tenant.</summary>
    public static bool CanUpdateConfiguration(TenantStatus current) => !current.Equals(TenantStatus.Deleted);
}
