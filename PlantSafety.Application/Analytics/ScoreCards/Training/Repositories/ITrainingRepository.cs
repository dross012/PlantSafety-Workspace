// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Training read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Training.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Training.Repositories;

/// <summary>Read-only repository for Training data. Implemented in the Infrastructure layer.</summary>
public interface ITrainingRepository
{
    /// <summary>Returns Training items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrainingItemDto>> GetTrainingScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
