// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHRQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.HR.Models;

namespace PlantSafety.Application.Compliance.Policy.HR.Queries;

/// <summary>Result of <see cref="GetHRQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HR items.</param>
public sealed record HRQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HRItemDto> Items);
