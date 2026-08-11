// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetErgonomicsMaterialHandlingQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Models;

namespace PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Queries;

/// <summary>Result of <see cref="GetErgonomicsMaterialHandlingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Ergonomics &amp; Material Handling items.</param>
public sealed record ErgonomicsMaterialHandlingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ErgonomicsMaterialHandlingItemDto> Items);