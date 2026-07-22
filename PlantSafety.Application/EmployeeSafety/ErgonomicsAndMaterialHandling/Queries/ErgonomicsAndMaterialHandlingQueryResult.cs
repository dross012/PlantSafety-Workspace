// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetErgonomicsAndMaterialHandlingQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Models;

namespace PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Queries;

/// <summary>Result of <see cref="GetErgonomicsAndMaterialHandlingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ErgonomicsAndMaterialHandling items.</param>
public sealed record ErgonomicsAndMaterialHandlingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ErgonomicsAndMaterialHandlingItemDto> Items);