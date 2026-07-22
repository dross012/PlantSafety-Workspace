// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetWorkOrderPriorityQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.WorkOrderPriority.Models;

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Queries;

/// <summary>Result of <see cref="GetWorkOrderPriorityQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkOrderPriority items.</param>
public sealed record WorkOrderPriorityQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkOrderPriorityItemDto> Items);