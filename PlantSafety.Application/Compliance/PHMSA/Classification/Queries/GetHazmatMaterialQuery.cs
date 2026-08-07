// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           GetHazmatMaterialQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single HazmatMaterial read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Queries;

/// <summary>Retrieves a single HazmatMaterial read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazmatMaterialId">Identity of the HazmatMaterial record to retrieve.</param>
public sealed record GetHazmatMaterialQuery(
    Guid TenantId,
    Guid HazmatMaterialId) : IRequest<Result<HazmatMaterialDto>>;
