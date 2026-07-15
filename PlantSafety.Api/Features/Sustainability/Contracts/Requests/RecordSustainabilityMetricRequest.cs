// ============================================================================
// File: RecordSustainabilityMetricRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST v1/sustainability/metrics,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.Sustainability.Contracts.Requests;

/// <summary>Request body for recording a new sustainability metric observation.</summary>
public sealed record RecordSustainabilityMetricRequest(
    Guid?    SiteId,
    string   MetricName,
    string   Category,
    double   Value,
    string   Unit,
    double?  Target,
    DateTime PeriodEndUtc);
