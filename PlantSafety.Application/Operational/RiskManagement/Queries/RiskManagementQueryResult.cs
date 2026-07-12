// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRiskManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.RiskManagement.Models;

namespace PlantSafety.Application.Operational.RiskManagement.Queries;

/// <summary>Result of <see cref="GetRiskManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RiskManagement items.</param>
public sealed record RiskManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RiskManagementItemDto> Items);
