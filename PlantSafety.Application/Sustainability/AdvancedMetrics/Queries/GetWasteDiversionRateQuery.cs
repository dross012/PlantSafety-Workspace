// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetWasteDiversionRateQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the waste diversion rate rollup for a ledger. Backs the
//                  Waste Diversion Rate dashboard. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

/// <summary>Retrieves the waste diversion rate rollup for a ledger.</summary>
public sealed record GetWasteDiversionRateQuery(
    Guid TenantId,
    Guid AdvancedMetricsLedgerId) : IRequest<Result<WasteDiversionRateDto>>;
