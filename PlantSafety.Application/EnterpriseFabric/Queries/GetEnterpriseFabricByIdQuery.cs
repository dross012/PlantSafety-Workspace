// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           GetEnterpriseFabricByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single EnterpriseFabric read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnterpriseFabric.Models;

namespace PlantSafety.Application.EnterpriseFabric.Queries;

/// <summary>Retrieves a single EnterpriseFabric read-model record by identity.</summary>
public sealed record GetEnterpriseFabricByIdQuery(
    Guid TenantId,
    Guid EnterpriseFabricId) : IRequest<Result<EnterpriseFabricItemDto>>;
