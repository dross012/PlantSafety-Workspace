// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           IErgonomicsWorkstationDesignRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ErgonomicsWorkstationDesign aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Interfaces;

/// <summary>Domain repository interface for the <see cref="ErgonomicsWorkstationDesign"/> aggregate root.</summary>
public interface IErgonomicsWorkstationDesignRepository
{
    /// <summary>Retrieves a ErgonomicsWorkstationDesign aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign?> GetByIdAsync(ErgonomicsWorkstationDesignId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ErgonomicsWorkstationDesign aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign>> ListAsync(ErgonomicsWorkstationDesignSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ErgonomicsWorkstationDesign aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ErgonomicsWorkstationDesign aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign aggregate, CancellationToken cancellationToken = default);
}