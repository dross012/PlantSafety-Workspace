/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IRepository.cs
    DESCRIPTION:        Generic repository abstraction for Intelligence aggregate roots. The Domain layer
                        owns this contract; Infrastructure implements it. Keeps the domain free of any
                        persistence technology. (DDD Repository pattern, Rule 2.6)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.Shared.Interfaces;

/// <summary>
/// Generic repository abstraction for Intelligence aggregate roots.
/// Concrete repository interfaces in each module extend this with aggregate-specific queries.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type for the aggregate.</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : struct
{
    /// <summary>Returns the aggregate with the given identifier, or <see langword="null"/> when not found.</summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new aggregate to the repository. Persisted when the Unit of Work commits.</summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing aggregate as modified. Persisted when the Unit of Work commits.</summary>
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
