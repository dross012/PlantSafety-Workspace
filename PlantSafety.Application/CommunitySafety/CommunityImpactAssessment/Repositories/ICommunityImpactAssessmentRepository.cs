// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the CommunityImpactAssessment read-model.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Repositories;

/// <summary>Read-only repository for CommunityImpactAssessment data. Implemented in the Infrastructure layer.</summary>
public interface ICommunityImpactAssessmentRepository
{
    /// <summary>Returns CommunityImpactAssessment items recorded for a given SafetyEvents incident.</summary>
    Task<IReadOnlyList<CommunityImpactAssessmentItemDto>> GetImpactAssessmentItemsByIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken);
}
