// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetAnalyzersQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Analyzers.Models;

namespace PlantSafety.Application.Integrations.Analyzers.Queries;

/// <summary>Result of <see cref="GetAnalyzersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Analyzers items.</param>
public sealed record AnalyzersQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AnalyzersItemDto> Items);
