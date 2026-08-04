// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           ICommunityNotificationRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer write repository abstraction for the CommunityNotification
//                  aggregate. Implemented in Infrastructure (Dapper, stored procedures only).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates.CommunityNotification;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Interfaces;

/// <summary>Write repository abstraction for the CommunityNotification aggregate.</summary>
public interface ICommunityNotificationRepository
{
    /// <summary>Loads a CommunityNotification aggregate by its identity, or null if not found.</summary>
    Task<Agg?> GetByIdAsync(CommunityNotificationId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly-created CommunityNotification aggregate.</summary>
    Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CommunityNotification aggregate.</summary>
    Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default);
}
