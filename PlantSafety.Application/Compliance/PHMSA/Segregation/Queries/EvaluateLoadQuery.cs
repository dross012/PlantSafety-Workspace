// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Application
//  File:           EvaluateLoadQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query that wraps
//                  PlantSafety.Domain.Compliance.PHMSA.Segregation.Services.SegregationEngine.
//                  EvaluateSegregation directly. There is no persisted aggregate for Segregation,
//                  so no repository is involved. Modeled as a query (side-effect-free calculation
//                  over caller-supplied input, no persistence) despite being exposed over an
//                  HTTP POST at the Api layer (the payload is too large for a query string).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Segregation.Queries;

/// <summary>Evaluates a proposed multi-hazmat load for segregation incompatibilities.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="LoadItems">The materials/hazard classes present in the proposed load.</param>
public sealed record EvaluateLoadQuery(
    Guid TenantId,
    IReadOnlyList<SegregationLoadItemDto> LoadItems) : IRequest<Result<SegregationEvaluationResultDto>>;
