// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IIncidentManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the IncidentManagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="IncidentManagement"/> aggregate root.</summary>
public interface IIncidentManagementRepository
{
    /// <summary>Retrieves a IncidentManagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<IncidentManagement?> GetByIdAsync(IncidentManagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all IncidentManagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<IncidentManagement>> ListAsync(IncidentManagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new IncidentManagement aggregate.</summary>
    Task AddAsync(IncidentManagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing IncidentManagement aggregate.</summary>
    Task UpdateAsync(IncidentManagement aggregate, CancellationToken cancellationToken = default);
}
