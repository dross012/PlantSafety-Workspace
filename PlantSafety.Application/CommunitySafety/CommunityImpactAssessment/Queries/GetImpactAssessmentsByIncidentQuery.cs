// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve all CommunityImpactAssessment records for a given SafetyEvents incident.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;

/// <summary>Retrieves all CommunityImpactAssessment records recorded for a given on-site incident.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="IncidentId">Weak reference to the SafetyEvents incident.</param>
public sealed record GetImpactAssessmentsByIncidentQuery(
    Guid TenantId,
    Guid IncidentId) : IRequest<Result<CommunityImpactAssessmentsQueryResult>>;
