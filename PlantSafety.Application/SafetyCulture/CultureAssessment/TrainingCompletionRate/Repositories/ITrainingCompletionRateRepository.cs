// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the TrainingCompletionRate read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Repositories;

/// <summary>Read-only repository for TrainingCompletionRate data. Implemented in the Infrastructure layer.</summary>
public interface ITrainingCompletionRateRepository
{
    /// <summary>Returns TrainingCompletionRate items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrainingCompletionRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}