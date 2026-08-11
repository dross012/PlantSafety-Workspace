// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Hazard Communication & Assessment Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Models;

namespace PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Repositories;

/// <summary>Read-only repository for Hazard Communication &amp; Assessment Policies data. Implemented in the Infrastructure layer.</summary>
public interface IHazardCommunicationAssessmentRepository
{
    /// <summary>Returns Hazard Communication &amp; Assessment Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardCommunicationAssessmentItemDto>> GetHazardCommunicationAssessmentAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}