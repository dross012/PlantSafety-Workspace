// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetFinancialsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Financials.Financials.Models;

namespace PlantSafety.Application.Financials.Financials.Queries;

/// <summary>Result of <see cref="GetFinancialsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Financials items.</param>
public sealed record FinancialsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<FinancialsItemDto> Items);
