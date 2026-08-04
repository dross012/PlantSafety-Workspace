// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetWorkloadTrendByShiftQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving workload-trend snapshots for a shift within a period. Backs
//                  the Cognitive Load by Shift/Role dashboard. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;

/// <summary>Retrieves workload-trend snapshots for a tenant/site/shift within a period.</summary>
public sealed record GetWorkloadTrendByShiftQuery(
    Guid TenantId,
    Guid? SiteId,
    string ShiftType,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc) : IRequest<Result<IReadOnlyList<WorkloadTrendByShiftDto>>>;
