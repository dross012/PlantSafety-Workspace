// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the AITraining read-model.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.AITraining.Models;

namespace PlantSafety.Application.Intelligence.AITraining.Repositories;

/// <summary>Read-only repository for AITraining data. Implemented in the Infrastructure layer.</summary>
public interface IAITrainingRepository
{
    /// <summary>Returns AITraining items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AITrainingItemDto>> GetTrainingJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}