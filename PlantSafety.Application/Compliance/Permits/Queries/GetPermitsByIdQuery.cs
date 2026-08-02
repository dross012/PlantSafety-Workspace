// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           GetPermitsByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Permits read-model record by identity, including its
//                  current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors PlantSafety.Application.RiskManagement.Queries.GetRiskByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Models;

namespace PlantSafety.Application.Compliance.Permits.Queries;

/// <summary>Retrieves a single Permits read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PermitsId">Identity of the Permits record to retrieve.</param>
public sealed record GetPermitsByIdQuery(
    Guid TenantId,
    Guid PermitsId) : IRequest<Result<PermitsItemDto>>;
