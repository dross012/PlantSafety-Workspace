// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPPEAssignmentQuery.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEAssignment.Models;

namespace PlantSafety.Application.PPE.PPEAssignment.Queries;

/// <summary>Result of <see cref="GetPPEAssignmentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPEAssignment items.</param>
public sealed record PPEAssignmentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPEAssignmentItemDto> Items);