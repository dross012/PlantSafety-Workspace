// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetMOCWorkflowsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Queries;

/// <summary>Result of <see cref="GetMOCWorkflowsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MOCWorkflows items.</param>
public sealed record MOCWorkflowsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MOCWorkflowsItemDto> Items);
