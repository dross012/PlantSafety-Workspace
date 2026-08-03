// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceRiskSummaryQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Predictive Dashboard's Risk Summary card. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the risk-summary card counts, derived from PredictiveMaintenanceAlert status buckets, for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
public sealed record GetPredictiveMaintenanceRiskSummaryQuery(Guid TenantId) : IRequest<Result<PredictiveMaintenanceRiskSummaryDto>>;
