// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetAllInvestigationsQuery.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.AllInvestigations.Models;

namespace PlantSafety.Application.Intelligence.AllInvestigations.Queries;

/// <summary>Result of <see cref="GetAllInvestigationsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AllInvestigations items.</param>
public sealed record AllInvestigationsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AllInvestigationsItemDto> Items);