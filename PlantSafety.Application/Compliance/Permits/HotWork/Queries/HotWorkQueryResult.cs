// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHotWorkQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.HotWork.Models;

namespace PlantSafety.Application.Compliance.Permits.HotWork.Queries;

/// <summary>Result of <see cref="GetHotWorkQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HotWork items.</param>
public sealed record HotWorkQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HotWorkItemDto> Items);
