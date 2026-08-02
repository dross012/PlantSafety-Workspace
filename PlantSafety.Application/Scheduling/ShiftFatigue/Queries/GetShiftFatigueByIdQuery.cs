// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           GetShiftFatigueByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ShiftFatigue read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.ShiftFatigue.Models;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Queries;

/// <summary>Retrieves a single ShiftFatigue read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ShiftFatigueId">Identity of the ShiftFatigue record to retrieve.</param>
public sealed record GetShiftFatigueByIdQuery(
    Guid TenantId,
    Guid ShiftFatigueId) : IRequest<Result<ShiftFatigueItemDto>>;
