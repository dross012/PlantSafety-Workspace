// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetDataLakeQuery.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.DataLake.Models;

namespace PlantSafety.Application.Intelligence.DataLake.Queries;

/// <summary>Result of <see cref="GetDataLakeQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching DataLake items.</param>
public sealed record DataLakeQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<DataLakeItemDto> Items);