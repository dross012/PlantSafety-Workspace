// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           GetEmergencyDrillPlanningByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single EmergencyDrillPlanning read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;

/// <summary>Retrieves a single EmergencyDrillPlanning read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EmergencyDrillPlanningId">Identity of the EmergencyDrillPlanning record to retrieve.</param>
public sealed record GetEmergencyDrillPlanningByIdQuery(
    Guid TenantId,
    Guid EmergencyDrillPlanningId) : IRequest<Result<EmergencyDrillPlanningItemDto>>;
