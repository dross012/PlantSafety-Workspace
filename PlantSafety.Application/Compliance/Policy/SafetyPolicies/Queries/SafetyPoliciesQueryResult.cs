// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSafetyPoliciesQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Models;

namespace PlantSafety.Application.Compliance.Policy.SafetyPolicies.Queries;

/// <summary>Result of <see cref="GetSafetyPoliciesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching SafetyPolicies items.</param>
public sealed record SafetyPoliciesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetyPoliciesItemDto> Items);
