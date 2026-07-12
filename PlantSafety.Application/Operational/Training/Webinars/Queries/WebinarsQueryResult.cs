// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetWebinarsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Webinars.Models;

namespace PlantSafety.Application.Operational.Training.Webinars.Queries;

/// <summary>Result of <see cref="GetWebinarsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Webinars items.</param>
public sealed record WebinarsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WebinarItemDto> Items);
