// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSeminarsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Seminars.Models;

namespace PlantSafety.Application.Operational.Training.Seminars.Queries;

/// <summary>Result of <see cref="GetSeminarsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Seminars items.</param>
public sealed record SeminarsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SeminarItemDto> Items);
