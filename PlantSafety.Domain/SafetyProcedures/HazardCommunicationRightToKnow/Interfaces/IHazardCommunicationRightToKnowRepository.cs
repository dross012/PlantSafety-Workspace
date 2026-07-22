// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           IHazardCommunicationRightToKnowRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazardCommunicationRightToKnow aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazardCommunicationRightToKnow"/> aggregate root.</summary>
public interface IHazardCommunicationRightToKnowRepository
{
    /// <summary>Retrieves a HazardCommunicationRightToKnow aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow?> GetByIdAsync(HazardCommunicationRightToKnowId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all HazardCommunicationRightToKnow aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow>> ListAsync(HazardCommunicationRightToKnowSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazardCommunicationRightToKnow aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HazardCommunicationRightToKnow aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow aggregate, CancellationToken cancellationToken = default);
}