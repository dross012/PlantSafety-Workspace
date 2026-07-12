// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetApprovalsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.ManagementOfChange.Approvals.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Approvals.Queries;

/// <summary>Result of <see cref="GetApprovalsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Approvals items.</param>
public sealed record ApprovalsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ApprovalsItemDto> Items);
