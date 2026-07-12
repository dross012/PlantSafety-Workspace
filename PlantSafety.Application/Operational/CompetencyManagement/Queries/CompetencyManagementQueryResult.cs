// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetCompetencyManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.CompetencyManagement.Models;

namespace PlantSafety.Application.Operational.CompetencyManagement.Queries;

/// <summary>Result of <see cref="GetCompetencyManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CompetencyManagement items.</param>
public sealed record CompetencyManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CompetencyManagementItemDto> Items);
