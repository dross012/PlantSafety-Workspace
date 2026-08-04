// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           ISabotageIndicatorRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SabotageIndicator aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.SabotageIndicator"/> aggregate root.</summary>
public interface ISabotageIndicatorRepository
{
    /// <summary>Retrieves a SabotageIndicator by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.SabotageIndicator?> GetByIdAsync(SabotageIndicatorId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all currently active (Open/UnderInvestigation) SabotageIndicators for a tenant.</summary>
    Task<IReadOnlyList<Aggregates.SabotageIndicator>> ListActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns other active indicators within the correlation window, for multi-source correlation analysis.</summary>
    Task<IReadOnlyList<Aggregates.SabotageIndicator>> ListForCorrelationWindowAsync(Guid tenantId, DateTime windowStartUtc, DateTime windowEndUtc, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SabotageIndicator.</summary>
    Task AddAsync(Aggregates.SabotageIndicator aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SabotageIndicator.</summary>
    Task UpdateAsync(Aggregates.SabotageIndicator aggregate, CancellationToken cancellationToken = default);
}
