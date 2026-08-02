// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           GetHumanFactorsByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single HumanFactors read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of
//                  the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.Models;

namespace PlantSafety.Application.HumanFactors.Queries;

/// <summary>Retrieves a single HumanFactors read-model record by identity.</summary>
public sealed record GetHumanFactorsByIdQuery(
    Guid TenantId,
    Guid HumanFactorsId) : IRequest<Result<HumanFactorsItemDto>>;
