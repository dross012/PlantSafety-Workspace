// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListLoadSecurementFailuresQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Queries;

/// <summary>Result of <see cref="ListLoadSecurementFailuresQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching failed LoadSecurementCheck items.</param>
public sealed record LoadSecurementCheckQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LoadSecurementCheckItemDto> Items);
