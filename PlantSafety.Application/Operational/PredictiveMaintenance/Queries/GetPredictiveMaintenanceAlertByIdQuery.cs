// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceAlertByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single PredictiveMaintenanceAlert read-model record by
//                  identity, including its current ETag, so a caller can read-before-write into
//                  one of the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves a single PredictiveMaintenanceAlert read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictiveMaintenanceAlertId">Identity of the PredictiveMaintenanceAlert record to retrieve.</param>
public sealed record GetPredictiveMaintenanceAlertByIdQuery(
    Guid TenantId,
    Guid PredictiveMaintenanceAlertId) : IRequest<Result<PredictiveMaintenanceAlertItemDto>>;
