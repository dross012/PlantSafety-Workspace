// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPPEManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PPEManagement.Models;

namespace PlantSafety.Application.Operational.PPEManagement.Queries;

/// <summary>Result of <see cref="GetPPEManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPEManagement items.</param>
public sealed record PPEManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPEManagementItemDto> Items);
