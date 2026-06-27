// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the PlanningProjects module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Abstractions;

/// <summary>Provides project scheduling and milestone dependency management.</summary>
public interface IProjectSchedulingService
{
    /// <summary>Returns milestone identifiers that are blocking project completion.</summary>
    Task<IReadOnlyList<Guid>> GetBlockingMilestoneIdsAsync(
        Guid tenantId,
        Guid projectId,
        CancellationToken cancellationToken);

    /// <summary>Returns <c>true</c> when all milestones for the project are in a terminal state.</summary>
    Task<bool> AreAllMilestonesCompleteAsync(
        Guid tenantId,
        Guid projectId,
        CancellationToken cancellationToken);
}
