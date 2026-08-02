// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           GetPreventiveMaintenanceTaskByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single PreventiveMaintenanceTask read-model record by
//                  identity, including its current ETag, so a caller can read-before-write into
//                  one of the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Queries;

/// <summary>Retrieves a single PreventiveMaintenanceTask read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PreventiveMaintenanceTaskId">Identity of the PreventiveMaintenanceTask record to retrieve.</param>
public sealed record GetPreventiveMaintenanceTaskByIdQuery(
    Guid TenantId,
    Guid PreventiveMaintenanceTaskId) : IRequest<Result<PreventiveMaintenanceTaskItemDto>>;
