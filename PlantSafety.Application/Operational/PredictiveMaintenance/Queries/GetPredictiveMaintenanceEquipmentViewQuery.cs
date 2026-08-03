// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceEquipmentViewQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Equipment Predictive View header. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the predictive-view header for a single equipment asset, derived from its most recent PredictiveMaintenanceAlert.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EquipmentId">Target equipment identifier.</param>
public sealed record GetPredictiveMaintenanceEquipmentViewQuery(Guid TenantId, Guid EquipmentId) : IRequest<Result<PredictiveMaintenanceEquipmentViewDto>>;
