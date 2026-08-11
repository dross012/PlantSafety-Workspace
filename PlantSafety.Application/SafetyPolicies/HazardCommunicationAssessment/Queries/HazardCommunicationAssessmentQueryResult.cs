// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHazardCommunicationAssessmentQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Models;

namespace PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Queries;

/// <summary>Result of <see cref="GetHazardCommunicationAssessmentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Hazard Communication &amp; Assessment Policies items.</param>
public sealed record HazardCommunicationAssessmentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardCommunicationAssessmentItemDto> Items);