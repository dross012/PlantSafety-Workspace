// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetIncidentResponseQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Models;

namespace PlantSafety.Application.Compliance.Procedures.IncidentResponse.Queries;

/// <summary>Result of <see cref="GetIncidentResponseQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentResponse items.</param>
public sealed record IncidentResponseQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentResponseItemDto> Items);
