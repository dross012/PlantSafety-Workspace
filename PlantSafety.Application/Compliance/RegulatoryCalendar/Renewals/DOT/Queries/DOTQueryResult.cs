// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetDOTQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.DOT.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.DOT.Queries;

/// <summary>Result of <see cref="GetDOTQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching DOT items.</param>
public sealed record DOTQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<DOTItemDto> Items);
