// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           GetHazmatContainerQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single HazmatContainer read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Queries;

/// <summary>Retrieves a single HazmatContainer read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazmatContainerId">Identity of the HazmatContainer record to retrieve.</param>
public sealed record GetHazmatContainerQuery(
    Guid TenantId,
    Guid HazmatContainerId) : IRequest<Result<HazmatContainerDto>>;
