// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetISO45001Query.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.ISO45001.Models;

namespace PlantSafety.Application.Compliance.Certification.ISO45001.Queries;

/// <summary>Result of <see cref="GetISO45001Query"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ISO45001 items.</param>
public sealed record ISO45001QueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ISO45001ItemDto> Items);
