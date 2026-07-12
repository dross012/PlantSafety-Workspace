// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetKPISnapshotsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Models;

namespace PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Queries;

/// <summary>Result of <see cref="GetKPISnapshotsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching KPISnapshots items.</param>
public sealed record KPISnapshotsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<KPISnapshotsItemDto> Items);
