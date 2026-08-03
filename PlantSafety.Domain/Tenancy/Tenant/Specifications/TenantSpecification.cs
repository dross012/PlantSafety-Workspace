// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantSpecification.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.3 Tenant Lifecycle Management)
//  SchemaVersion:  2026.1
//  Description:    Reusable predicates over the Tenant aggregate for Application-layer
//                  eligibility checks (e.g. before dispatching ArchiveTenantDataCommand or
//                  ExportTenantDataCommand). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Tenancy.Tenant.Enums;

namespace PlantSafety.Domain.Tenancy.Tenant.Specifications;

/// <summary>Reusable eligibility predicates over the Tenant aggregate.</summary>
public static class TenantSpecification
{
    /// <summary>True when the tenant is serving live traffic (Active status).</summary>
    public static bool IsOperational(Aggregates.Tenant tenant) => tenant.Status.Equals(TenantStatus.Active);

    /// <summary>True when the tenant is eligible for a full data export (Active, Suspended, or Archived -- not yet Deleted).</summary>
    public static bool IsExportable(Aggregates.Tenant tenant) => !tenant.Status.Equals(TenantStatus.Deleted);

    /// <summary>True when the tenant's data is eligible for archival (Active or Suspended).</summary>
    public static bool IsArchivable(Aggregates.Tenant tenant) =>
        tenant.Status.Equals(TenantStatus.Active) || tenant.Status.Equals(TenantStatus.Suspended);
}
