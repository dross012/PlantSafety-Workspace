// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetTransportIncidentHistoryQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Queries;

/// <summary>Result of <see cref="GetTransportIncidentHistoryQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching TransportIncident items.</param>
public sealed record TransportIncidentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TransportIncidentItemDto> Items);
