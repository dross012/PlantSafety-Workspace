// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Persistence
//  File:           FacilityRepository.cs
//  Created:        2026-08-08
//  Description:    In-memory IFacilityRepository implementation, backed by
//                  IFacilityStore. Rehydrates a fresh FacilityAggregate on
//                  every FindAsync call (no aggregate-instance caching), so
//                  each caller gets its own DomainEvents list.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Infrastructure.Persistence;

/// <summary>In-memory <see cref="IFacilityRepository"/> implementation, backed by <see cref="IFacilityStore"/>.</summary>
public sealed class FacilityRepository(IFacilityStore store) : IFacilityRepository
{
    /// <inheritdoc />
    public Task<FacilityAggregate?> FindAsync(FacilityId facilityId, CancellationToken cancellationToken = default)
    {
        FacilityState? state = store.Find(facilityId);
        return Task.FromResult(state is null ? null : FacilityAggregate.Rehydrate(state));
    }

    /// <inheritdoc />
    public Task SaveAsync(FacilityAggregate aggregate, CancellationToken cancellationToken = default)
    {
        store.Save(aggregate.State);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<FacilityAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FacilityAggregate> aggregates = store.GetAll().Select(FacilityAggregate.Rehydrate).ToArray();
        return Task.FromResult(aggregates);
    }
}
