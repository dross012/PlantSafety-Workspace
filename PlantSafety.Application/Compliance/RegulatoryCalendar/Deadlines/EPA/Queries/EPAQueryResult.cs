// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEPAQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Queries;

/// <summary>Result of <see cref="GetEPAQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EPA items.</param>
public sealed record EPAQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EPAItemDto> Items);
