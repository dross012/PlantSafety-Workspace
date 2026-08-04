// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           ICommunityContactRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer write repository abstraction for the CommunityContact aggregate.
//                  Implemented in Infrastructure (Dapper, stored procedures only). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Interfaces;

/// <summary>Write repository abstraction for the CommunityContact aggregate.</summary>
public interface ICommunityContactRepository
{
    /// <summary>Loads a CommunityContact aggregate by its identity, or null if not found.</summary>
    Task<Agg?> GetByIdAsync(CommunityContactId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly-created CommunityContact aggregate.</summary>
    Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CommunityContact aggregate.</summary>
    Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default);
}
