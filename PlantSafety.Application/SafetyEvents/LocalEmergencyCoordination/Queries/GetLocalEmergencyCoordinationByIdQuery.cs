// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetLocalEmergencyCoordinationByIdQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single LocalEmergencyCoordination case by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors GetEmergencyManagementByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

/// <summary>Retrieves a single LocalEmergencyCoordination case read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="LocalEmergencyCoordinationId">Identity of the case to retrieve.</param>
public sealed record GetLocalEmergencyCoordinationByIdQuery(
    Guid TenantId,
    Guid LocalEmergencyCoordinationId) : IRequest<Result<LocalEmergencyCoordinationItemDto>>;
