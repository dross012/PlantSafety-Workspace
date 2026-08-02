// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           GetPolicyByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Policy read-model record by identity, including its
//                  current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Models;

namespace PlantSafety.Application.Compliance.Policy.Queries;

/// <summary>Retrieves a single Policy read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PolicyId">Identity of the Policy record to retrieve.</param>
public sealed record GetPolicyByIdQuery(
    Guid TenantId,
    Guid PolicyId) : IRequest<Result<PolicyItemDto>>;
