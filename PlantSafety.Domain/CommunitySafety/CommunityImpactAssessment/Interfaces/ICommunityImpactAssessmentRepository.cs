// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           ICommunityImpactAssessmentRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer write repository abstraction for the CommunityImpactAssessment
//                  aggregate. Implemented in Infrastructure (Dapper, stored procedures only).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Aggregates.CommunityImpactAssessment;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Interfaces;

/// <summary>Write repository abstraction for the CommunityImpactAssessment aggregate.</summary>
public interface ICommunityImpactAssessmentRepository
{
    /// <summary>Loads a CommunityImpactAssessment aggregate by its identity, or null if not found.</summary>
    Task<Agg?> GetByIdAsync(CommunityImpactAssessmentId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly-created CommunityImpactAssessment aggregate.</summary>
    Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CommunityImpactAssessment aggregate.</summary>
    Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default);
}
