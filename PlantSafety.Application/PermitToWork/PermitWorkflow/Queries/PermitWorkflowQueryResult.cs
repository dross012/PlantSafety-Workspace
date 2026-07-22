// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPermitWorkflowQuery.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitWorkflow.Models;

namespace PlantSafety.Application.PermitToWork.PermitWorkflow.Queries;

/// <summary>Result of <see cref="GetPermitWorkflowQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitWorkflow items.</param>
public sealed record PermitWorkflowQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitWorkflowItemDto> Items);