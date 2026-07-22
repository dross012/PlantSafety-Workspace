// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetControlMeasuresHierarchyQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Models;

namespace PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Queries;

/// <summary>Result of <see cref="GetControlMeasuresHierarchyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Control Measures (Hierarchy of Controls) items.</param>
public sealed record ControlMeasuresHierarchyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ControlMeasuresHierarchyItemDto> Items);