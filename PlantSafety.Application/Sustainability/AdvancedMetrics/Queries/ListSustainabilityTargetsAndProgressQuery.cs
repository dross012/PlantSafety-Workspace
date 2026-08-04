// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           ListSustainabilityTargetsAndProgressQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving all sustainability targets and their tracked progress for a
//                  ledger. Backs the Target Progress Tracker dashboard. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

/// <summary>Retrieves all sustainability targets and their tracked progress for a ledger.</summary>
public sealed record ListSustainabilityTargetsAndProgressQuery(
    Guid TenantId,
    Guid AdvancedMetricsLedgerId) : IRequest<Result<IReadOnlyList<SustainabilityTargetProgressDto>>>;
