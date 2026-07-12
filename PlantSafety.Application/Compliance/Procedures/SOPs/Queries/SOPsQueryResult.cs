// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSOPsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.SOPs.Models;

namespace PlantSafety.Application.Compliance.Procedures.SOPs.Queries;

/// <summary>Result of <see cref="GetSOPsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching SOPs items.</param>
public sealed record SOPsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SOPsItemDto> Items);
