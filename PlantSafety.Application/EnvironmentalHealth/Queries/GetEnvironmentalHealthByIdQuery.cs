// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           GetEnvironmentalHealthByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single EnvironmentalHealth read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of
//                  the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.Models;

namespace PlantSafety.Application.EnvironmentalHealth.Queries;

/// <summary>Retrieves a single EnvironmentalHealth read-model record by identity.</summary>
public sealed record GetEnvironmentalHealthByIdQuery(
    Guid TenantId,
    Guid EnvironmentalHealthId) : IRequest<Result<EnvironmentalHealthItemDto>>;
