// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Courses read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Courses.Models;

namespace PlantSafety.Application.Operational.Training.Courses.Repositories;

/// <summary>Read-only repository for Courses data. Implemented in the Infrastructure layer.</summary>
public interface ICoursesRepository
{
    /// <summary>Returns Courses items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CourseItemDto>> GetCoursesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
