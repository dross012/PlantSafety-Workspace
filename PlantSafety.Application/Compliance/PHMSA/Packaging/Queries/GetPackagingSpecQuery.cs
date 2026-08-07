// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           GetPackagingSpecQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single PackagingSpecification read-model record by
//                  identity, including its current ETag, so a caller can read-before-write into
//                  one of the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;

/// <summary>Retrieves a single PackagingSpecification read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PackagingSpecificationId">Identity of the PackagingSpecification record to retrieve.</param>
public sealed record GetPackagingSpecQuery(
    Guid TenantId,
    Guid PackagingSpecificationId) : IRequest<Result<PackagingSpecificationDto>>;
