// ============================================================================
// File: SustainabilityMetricResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTOs for the Sustainability module.
// ============================================================================

namespace PlantSafety.Api.Features.Sustainability.Contracts.Responses;

/// <summary>Response payload for a single sustainability metric record.</summary>
public sealed record SustainabilityMetricResponse(
    Guid     MetricId,
    Guid     TenantId,
    Guid?    SiteId,
    string   MetricName,
    string   Category,
    double   Value,
    string   Unit,
    double?  Target,
    DateTime PeriodEndUtc,
    string   Status);

/// <summary>Envelope response payload for the sustainability metric listing.</summary>
public sealed record SustainabilityListResponse(
    Guid                                        TenantId,
    Guid?                                       SiteId,
    DateTime                                    GeneratedAt,
    IReadOnlyList<SustainabilityMetricResponse> Items);

/// <summary>Response payload returned after recording a sustainability metric.</summary>
/// <param name="MetricId">Identifier of the newly recorded metric.</param>
public sealed record RecordSustainabilityMetricResponse(Guid MetricId);
