// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Department read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Department.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Department.Repositories;

/// <summary>Read-only repository for Department data. Implemented in the Infrastructure layer.</summary>
public interface IDepartmentRepository
{
    /// <summary>Returns Department items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DepartmentItemDto>> GetDepartmentScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
