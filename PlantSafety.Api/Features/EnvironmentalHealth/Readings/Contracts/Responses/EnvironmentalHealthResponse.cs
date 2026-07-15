// ============================================================================
// File: EnvironmentalHealthResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTO for a single Environmental Health reading.
// ============================================================================

namespace PlantSafety.Api.Features.EnvironmentalHealth.Readings.Contracts.Responses;

/// <summary>Response payload for a single Environmental Health reading.</summary>
public sealed record EnvironmentalHealthResponse(
    Guid           EnvironmentalHealthId,
    Guid           TenantId,
    Guid?          SiteId,
    string         MetricType,
    decimal        MeasurementValue,
    string         Unit,
    decimal        Threshold,
    string         Status,
    DateTimeOffset RecordedAtUtc);
