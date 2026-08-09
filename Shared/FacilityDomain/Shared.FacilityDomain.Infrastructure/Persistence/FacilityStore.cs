// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Persistence
//  File:           FacilityStore.cs
//  Created:        2026-08-08
//  Description:    Thread-safe, in-memory, process-local store of
//                  FacilityState keyed by FacilityId. Backs FacilityRepository.
//                  Mirrors the ConcurrentDictionary-backed store pattern
//                  already established across this codebase (e.g.
//                  Reporting.Infrastructure's InMemoryAccidentImpactStore).
// ============================================================================
using System.Collections.Concurrent;

using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Infrastructure.Persistence;

/// <summary>Thread-safe, in-memory access to persisted <see cref="FacilityState"/> snapshots.</summary>
public interface IFacilityStore
{
    /// <summary>Saves (creates or overwrites) the given state, keyed by its FacilityId.</summary>
    void Save(FacilityState state);

    /// <summary>Finds the state for the given facility id, or null if none exists.</summary>
    FacilityState? Find(FacilityId facilityId);

    /// <summary>Returns every persisted state, in no particular order.</summary>
    IReadOnlyList<FacilityState> GetAll();
}

/// <summary>Thread-safe, in-memory, process-local <see cref="IFacilityStore"/> implementation.</summary>
public sealed class FacilityStore : IFacilityStore
{
    private readonly ConcurrentDictionary<Guid, FacilityState> _statesByFacilityId = new();

    /// <inheritdoc />
    public void Save(FacilityState state) => _statesByFacilityId[state.FacilityId.Value] = state;

    /// <inheritdoc />
    public FacilityState? Find(FacilityId facilityId) => _statesByFacilityId.GetValueOrDefault(facilityId.Value);

    /// <inheritdoc />
    public IReadOnlyList<FacilityState> GetAll() => _statesByFacilityId.Values.ToArray();
}
