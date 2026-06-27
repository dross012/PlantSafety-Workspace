// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IRepository.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Generic repository contract for aggregate persistence.
//                  All repositories MUST use stored procedures via Dapper.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Generic repository contract for aggregate-root persistence.
/// Implementations MUST use stored procedures and MUST NOT use inline SQL.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strong-typed aggregate identity.</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : IAggregateRoot
{
    /// <summary>Retrieves an aggregate by its identity and tenant scope.</summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new aggregate.</summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing aggregate.</summary>
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
