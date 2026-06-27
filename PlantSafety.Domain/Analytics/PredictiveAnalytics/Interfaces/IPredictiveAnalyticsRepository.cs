// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           IPredictiveAnalyticsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PredictiveAnalytics aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;

/// <summary>Domain repository interface for the <see cref="PredictiveAnalytics"/> aggregate root.</summary>
public interface IPredictiveAnalyticsRepository
{
    /// <summary>Retrieves a PredictiveAnalytics aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PredictiveAnalytics?> GetByIdAsync(PredictiveAnalyticsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PredictiveAnalytics aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PredictiveAnalytics>> ListAsync(PredictiveAnalyticsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PredictiveAnalytics aggregate.</summary>
    Task AddAsync(PredictiveAnalytics aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PredictiveAnalytics aggregate.</summary>
    Task UpdateAsync(PredictiveAnalytics aggregate, CancellationToken cancellationToken = default);
}
