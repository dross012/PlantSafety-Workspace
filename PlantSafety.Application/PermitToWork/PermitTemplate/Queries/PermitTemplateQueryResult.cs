// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPermitTemplateQuery.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitTemplate.Models;

namespace PlantSafety.Application.PermitToWork.PermitTemplate.Queries;

/// <summary>Result of <see cref="GetPermitTemplateQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitTemplate items.</param>
public sealed record PermitTemplateQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitTemplateItemDto> Items);