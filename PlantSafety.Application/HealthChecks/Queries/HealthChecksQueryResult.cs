// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHealthChecksQuery.
// SchemaVersion: 1

using PlantSafety.Application.HealthChecks.Models;

namespace PlantSafety.Application.HealthChecks.Queries;

/// <summary>Result of <see cref="GetHealthChecksQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HealthChecks items.</param>
public sealed record HealthChecksQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HealthCheckItemDto> Items);
