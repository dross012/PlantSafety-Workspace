// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetCrisisManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;

/// <summary>Result of <see cref="GetCrisisManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CrisisManagement items.</param>
public sealed record CrisisManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CrisisManagementItemDto> Items);
