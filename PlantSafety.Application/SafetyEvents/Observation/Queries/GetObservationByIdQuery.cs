// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           GetObservationByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Observation read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.Observation.Models;

namespace PlantSafety.Application.SafetyEvents.Observation.Queries;

/// <summary>Retrieves a single Observation read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ObservationId">Identity of the Observation record to retrieve.</param>
public sealed record GetObservationByIdQuery(
    Guid TenantId,
    Guid ObservationId) : IRequest<Result<ObservationItemDto>>;
