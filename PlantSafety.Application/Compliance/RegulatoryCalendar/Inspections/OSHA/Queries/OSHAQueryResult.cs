// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetOSHAQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Queries;

/// <summary>Result of <see cref="GetOSHAQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching OSHA items.</param>
public sealed record OSHAQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<OSHAItemDto> Items);
