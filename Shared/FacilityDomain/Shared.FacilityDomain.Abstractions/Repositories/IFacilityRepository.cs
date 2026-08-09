// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Repositories
//  File:           IFacilityRepository.cs
//  Created:        2026-08-08
//  Description:    Aggregate-level persistence for FacilityAggregate. The
//                  in-memory implementation lives in
//                  Shared.FacilityDomain.Infrastructure (Phase B.1 says
//                  "in-memory for now" -- a real backing store is a future
//                  phase, not this one).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Repositories;

/// <summary>Aggregate-level persistence for <see cref="FacilityAggregate"/>.</summary>
public interface IFacilityRepository
{
    /// <summary>Finds the facility with the given id, or null if none exists.</summary>
    Task<FacilityAggregate?> FindAsync(FacilityId facilityId, CancellationToken cancellationToken = default);

    /// <summary>Persists the aggregate's current state (create or overwrite).</summary>
    Task SaveAsync(FacilityAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Returns every persisted facility, in no particular order.</summary>
    Task<IReadOnlyList<FacilityAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}
