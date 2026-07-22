// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetConfinedSpaceEntryQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Models;

namespace PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Queries;

/// <summary>Result of <see cref="GetConfinedSpaceEntryQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Confined Space Entry Policies items.</param>
public sealed record ConfinedSpaceEntryQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ConfinedSpaceEntryItemDto> Items);