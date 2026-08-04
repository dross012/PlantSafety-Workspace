// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetImpactAssessmentsByIncidentQuery.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;

/// <summary>Result of <see cref="GetImpactAssessmentsByIncidentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="IncidentId">Weak reference to the SafetyEvents incident.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CommunityImpactAssessment items.</param>
public sealed record CommunityImpactAssessmentsQueryResult(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<CommunityImpactAssessmentItemDto> Items);
