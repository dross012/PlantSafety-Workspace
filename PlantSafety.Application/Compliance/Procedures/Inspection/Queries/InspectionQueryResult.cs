// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetInspectionQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Inspection.Models;

namespace PlantSafety.Application.Compliance.Procedures.Inspection.Queries;

/// <summary>Result of <see cref="GetInspectionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Inspection items.</param>
public sealed record InspectionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<InspectionItemDto> Items);
