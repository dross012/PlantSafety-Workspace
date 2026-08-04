// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetCarbonFootprintByPeriodQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the Scope 1/2/3 carbon footprint rollup for a tenant/site's
//                  reporting period. Backs the Carbon Footprint Trend dashboard.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

/// <summary>Retrieves the Scope 1/2/3 carbon footprint rollup for a reporting period.</summary>
public sealed record GetCarbonFootprintByPeriodQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc) : IRequest<Result<CarbonFootprintByPeriodDto>>;
