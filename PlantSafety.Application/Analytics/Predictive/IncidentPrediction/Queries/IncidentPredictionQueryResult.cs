// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetIncidentPredictionQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Models;

namespace PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Queries;

/// <summary>Result of <see cref="GetIncidentPredictionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentPrediction items.</param>
public sealed record IncidentPredictionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentPredictionItemDto> Items);
