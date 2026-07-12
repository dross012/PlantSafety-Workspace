// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetImportQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Import.Models;

namespace PlantSafety.Application.Integrations.Import.Queries;

/// <summary>Result of <see cref="GetImportQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Import items.</param>
public sealed record ImportQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ImportItemDto> Items);
