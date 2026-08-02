// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           GetCultureByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Culture read-model record by identity, including its
//                  current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Distinct from the pre-existing, unimplemented GetCultureQuery record
//                  (Application.Analytics.Culture.Queries.GetCultureQuery), which carries no
//                  IRequest<T> contract and has no handler. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.Models;

namespace PlantSafety.Application.Analytics.Culture.Queries;

/// <summary>Retrieves a single Culture read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CultureId">Identity of the Culture record to retrieve.</param>
public sealed record GetCultureByIdQuery(
    Guid TenantId,
    Guid CultureId) : IRequest<Result<CultureItemDto>>;
