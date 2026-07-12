// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetCoursesQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Courses.Models;

namespace PlantSafety.Application.Operational.Training.Courses.Queries;

/// <summary>Result of <see cref="GetCoursesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Courses items.</param>
public sealed record CoursesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CourseItemDto> Items);
