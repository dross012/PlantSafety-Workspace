// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetISO14001Query.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.ISO14001.Models;

namespace PlantSafety.Application.Compliance.Certification.ISO14001.Queries;

/// <summary>Result of <see cref="GetISO14001Query"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ISO14001 items.</param>
public sealed record ISO14001QueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ISO14001ItemDto> Items);
