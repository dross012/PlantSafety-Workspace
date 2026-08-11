// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLotoConfinedSpacesQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Models;

namespace PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Queries;

/// <summary>Result of <see cref="GetLotoConfinedSpacesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Lockout/Tagout (LOTO) &amp; Confined Spaces items.</param>
public sealed record LotoConfinedSpacesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LotoConfinedSpacesItemDto> Items);