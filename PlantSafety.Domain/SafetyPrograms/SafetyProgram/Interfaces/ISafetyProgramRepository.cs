// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           ISafetyProgramRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SafetyProgram aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.SafetyProgram"/> aggregate root.</summary>
public interface ISafetyProgramRepository
{
    /// <summary>Retrieves a SafetyProgram aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.SafetyProgram?> GetByIdAsync(SafetyProgramId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns a page of SafetyProgram aggregates for the given tenant, optionally filtered by status.</summary>
    Task<IReadOnlyList<Aggregates.SafetyProgram>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        SafetyProgramStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the total count of SafetyProgram aggregates matching the given filter (before paging).</summary>
    Task<int> CountAsync(
        Guid tenantId,
        Guid? siteId,
        SafetyProgramStatus? statusFilter,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a new SafetyProgram aggregate.</summary>
    Task AddAsync(Aggregates.SafetyProgram aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SafetyProgram aggregate.</summary>
    Task UpdateAsync(Aggregates.SafetyProgram aggregate, CancellationToken cancellationToken = default);
}
