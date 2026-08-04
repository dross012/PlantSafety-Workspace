// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           IPublicAlertBroadcastRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PublicAlertBroadcast aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;

/// <summary>Domain repository interface for the <see cref="PublicAlertBroadcast"/> aggregate root.</summary>
public interface IPublicAlertBroadcastRepository
{
    /// <summary>Retrieves a PublicAlertBroadcast aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast?> GetByIdAsync(PublicAlertBroadcastId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PublicAlertBroadcast aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast>> ListAsync(PublicAlertBroadcastSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PublicAlertBroadcast aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PublicAlertBroadcast aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast aggregate, string? eTag, CancellationToken cancellationToken = default);
}
