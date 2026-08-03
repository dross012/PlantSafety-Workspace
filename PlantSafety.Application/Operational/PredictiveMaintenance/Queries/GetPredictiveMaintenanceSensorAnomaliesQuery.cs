// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceSensorAnomaliesQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Sensor Data Explorer's detected-anomalies sub-resource, backed
//                  by the real Observability.Telemetry.Services.TelemetryAnomalyDetector.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves detected sensor anomalies for the Sensor Data Explorer.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EquipmentId">Optional equipment filter. Null uses the tenant's most recently predicted equipment.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetPredictiveMaintenanceSensorAnomaliesQuery(
    Guid TenantId,
    Guid? EquipmentId,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>>;
