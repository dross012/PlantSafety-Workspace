// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           ICostCenterMappingRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction for CostCenterMapping reference data. Implemented in the
//                  Infrastructure layer with a real Dapper-backed table. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;

/// <summary>Repository for <see cref="CostCenterMapping"/> reference data.</summary>
public interface ICostCenterMappingRepository
{
    /// <summary>
    /// Retrieves the cost center mapping for <paramref name="tenantId"/>/<paramref name="siteId"/>,
    /// falling back to the tenant-wide (null-site) mapping when no site-specific mapping exists.
    /// </summary>
    Task<CostCenterMapping?> GetAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);

    /// <summary>Creates or replaces the cost center mapping for the given Tenant/Site.</summary>
    Task UpsertAsync(CostCenterMapping mapping, CancellationToken cancellationToken);
}
