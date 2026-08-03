// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyInsightsRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the GlobalSafetyInsightsAggregate root. Every
//                  method is TenantId-scoped -- zero cross-tenant leakage (Phase 8 constraint).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Specifications;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Domain repository interface for the GlobalSafetyInsightsAggregate root.</summary>
public interface IGlobalSafetyInsightsRepository
{
    /// <summary>Retrieves a snapshot by identity, scoped to the given tenant, or <see langword="null"/> if not found/not owned by that tenant.</summary>
    Task<GlobalSafetyInsightsAggregate?> GetByIdAsync(GlobalSafetyInsightsId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the most recently computed snapshot for the given tenant, or <see langword="null"/> if none exists.</summary>
    Task<GlobalSafetyInsightsAggregate?> GetLatestAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns every snapshot for the given tenant matching the specification.</summary>
    Task<IReadOnlyList<GlobalSafetyInsightsAggregate>> ListAsync(Guid tenantId, GlobalSafetyInsightsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly computed snapshot. Snapshots are append-only -- there is no UpdateAsync.</summary>
    Task AddAsync(GlobalSafetyInsightsAggregate aggregate, CancellationToken cancellationToken = default);
}
