// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           GetNearMissesByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single NearMisses read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Queries
//                  .GetPredictiveMaintenanceAlertByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.NearMisses.Models;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Queries;

/// <summary>Retrieves a single NearMisses read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="NearMissesId">Identity of the NearMisses record to retrieve.</param>
public sealed record GetNearMissesByIdQuery(
    Guid TenantId,
    Guid NearMissesId) : IRequest<Result<NearMissesDetailDto>>;
