// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           IAutomatedIncidentDetectionRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AutomatedIncidentDetection aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Interfaces;

/// <summary>Domain repository interface for the <see cref="AutomatedIncidentDetection"/> aggregate root.</summary>
public interface IAutomatedIncidentDetectionRepository
{
    /// <summary>Retrieves a AutomatedIncidentDetection aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection?> GetByIdAsync(AutomatedIncidentDetectionId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AutomatedIncidentDetection aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection>> ListAsync(AutomatedIncidentDetectionSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AutomatedIncidentDetection aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AutomatedIncidentDetection aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection aggregate, CancellationToken cancellationToken = default);
}