// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceEquipmentHealthQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Predictive Dashboard's Equipment Health card. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the equipment-health card percentages, derived from each equipment's most recent alert status, for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
public sealed record GetPredictiveMaintenanceEquipmentHealthQuery(Guid TenantId) : IRequest<Result<PredictiveMaintenanceEquipmentHealthDto>>;
