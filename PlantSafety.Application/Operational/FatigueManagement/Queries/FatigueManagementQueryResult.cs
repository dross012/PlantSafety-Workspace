// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetFatigueManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.FatigueManagement.Models;

namespace PlantSafety.Application.Operational.FatigueManagement.Queries;

/// <summary>Result of <see cref="GetFatigueManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching FatigueManagement items.</param>
public sealed record FatigueManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<FatigueManagementItemDto> Items);
