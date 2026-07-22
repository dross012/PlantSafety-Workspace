// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEnvironmentalChemicalSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Models;

namespace PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Queries;

/// <summary>Result of <see cref="GetEnvironmentalChemicalSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Environmental & Chemical Safety Policies items.</param>
public sealed record EnvironmentalChemicalSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalChemicalSafetyItemDto> Items);