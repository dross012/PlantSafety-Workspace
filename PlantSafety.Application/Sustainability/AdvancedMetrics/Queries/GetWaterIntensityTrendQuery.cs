// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetWaterIntensityTrendQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the water-intensity trend points for a ledger. Backs the
//                  Energy/Water Intensity Board dashboard. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

/// <summary>Retrieves the water-intensity trend points for a ledger.</summary>
public sealed record GetWaterIntensityTrendQuery(
    Guid TenantId,
    Guid AdvancedMetricsLedgerId) : IRequest<Result<IReadOnlyList<WaterIntensityTrendPointDto>>>;
