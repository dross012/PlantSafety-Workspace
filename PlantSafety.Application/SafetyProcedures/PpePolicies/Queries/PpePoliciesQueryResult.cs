// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPpePoliciesQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.PpePolicies.Models;

namespace PlantSafety.Application.SafetyProcedures.PpePolicies.Queries;

/// <summary>Result of <see cref="GetPpePoliciesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PpePolicies items.</param>
public sealed record PpePoliciesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PpePoliciesItemDto> Items);