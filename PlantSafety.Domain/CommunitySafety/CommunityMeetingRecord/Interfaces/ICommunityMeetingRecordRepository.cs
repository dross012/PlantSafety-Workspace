// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           ICommunityMeetingRecordRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer write repository abstraction for the CommunityMeetingRecord
//                  aggregate. Implemented in Infrastructure (Dapper, stored procedures only).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates.CommunityMeetingRecord;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Interfaces;

/// <summary>Write repository abstraction for the CommunityMeetingRecord aggregate.</summary>
public interface ICommunityMeetingRecordRepository
{
    /// <summary>Loads a CommunityMeetingRecord aggregate by its identity, or null if not found.</summary>
    Task<Agg?> GetByIdAsync(CommunityMeetingRecordId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly-created CommunityMeetingRecord aggregate.</summary>
    Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CommunityMeetingRecord aggregate.</summary>
    Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default);
}
