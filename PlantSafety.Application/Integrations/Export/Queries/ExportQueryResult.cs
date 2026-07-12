// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetExportQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Export.Models;

namespace PlantSafety.Application.Integrations.Export.Queries;

/// <summary>Result of <see cref="GetExportQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Export items.</param>
public sealed record ExportQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ExportItemDto> Items);
