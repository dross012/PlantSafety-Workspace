// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           GetAuditingByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Auditing read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.Models;

namespace PlantSafety.Application.Compliance.Auditing.Queries;

/// <summary>Retrieves a single Auditing read-model record by identity.</summary>
public sealed record GetAuditingByIdQuery(
    Guid TenantId,
    Guid AuditingId) : IRequest<Result<AuditingItemDto>>;
