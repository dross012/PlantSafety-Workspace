// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHousekeepingErgonomicsQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Models;

namespace PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Queries;

/// <summary>Result of <see cref="GetHousekeepingErgonomicsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Housekeeping & Ergonomics Policies items.</param>
public sealed record HousekeepingErgonomicsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HousekeepingErgonomicsItemDto> Items);