// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           GetSustainabilityByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Sustainability read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.Models;

namespace PlantSafety.Application.Sustainability.Queries;

/// <summary>Retrieves a single Sustainability read-model record by identity.</summary>
public sealed record GetSustainabilityByIdQuery(
    Guid TenantId,
    Guid SustainabilityId) : IRequest<Result<SustainabilityItemDto>>;
