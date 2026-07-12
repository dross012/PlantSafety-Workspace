// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetAdministrationQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Administration.Models;

namespace PlantSafety.Application.CrossDomain.Administration.Queries;

/// <summary>Result of <see cref="GetAdministrationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Administration items.</param>
public sealed record AdministrationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AdministrationItemDto> Items);
