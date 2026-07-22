// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           ILotoProceduresRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LotoProcedures aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Interfaces;

/// <summary>Domain repository interface for the <see cref="LotoProcedures"/> aggregate root.</summary>
public interface ILotoProceduresRepository
{
    /// <summary>Retrieves a LotoProcedures aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures?> GetByIdAsync(LotoProceduresId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all LotoProcedures aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures>> ListAsync(LotoProceduresSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LotoProcedures aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing LotoProcedures aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures aggregate, CancellationToken cancellationToken = default);
}