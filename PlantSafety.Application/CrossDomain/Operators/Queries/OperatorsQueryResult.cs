// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetOperatorsQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Operators.Models;

namespace PlantSafety.Application.CrossDomain.Operators.Queries;

/// <summary>Result of <see cref="GetOperatorsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Operators items.</param>
public sealed record OperatorsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<OperatorsItemDto> Items);
