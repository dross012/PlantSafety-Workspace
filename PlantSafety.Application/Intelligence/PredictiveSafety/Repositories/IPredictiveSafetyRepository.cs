// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the PredictiveSafety read-model.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

/// <summary>Read-only repository for PredictiveSafety data. Implemented in the Infrastructure layer.</summary>
public interface IPredictiveSafetyRepository
{
    /// <summary>Returns PredictiveSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PredictiveSafetyItemDto>> GetPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}