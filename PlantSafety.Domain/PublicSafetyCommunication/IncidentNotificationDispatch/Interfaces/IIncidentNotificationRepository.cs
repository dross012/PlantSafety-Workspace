// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IIncidentNotificationRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the IncidentNotification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;

/// <summary>Domain repository interface for the <see cref="IncidentNotification"/> aggregate root.</summary>
public interface IIncidentNotificationRepository
{
    /// <summary>Retrieves an IncidentNotification aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification?> GetByIdAsync(IncidentNotificationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all IncidentNotification aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification>> ListAsync(IncidentNotificationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new IncidentNotification aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing IncidentNotification aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification aggregate, string? eTag, CancellationToken cancellationToken = default);
}
