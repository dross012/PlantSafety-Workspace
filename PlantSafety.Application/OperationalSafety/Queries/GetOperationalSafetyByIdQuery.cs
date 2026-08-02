// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           GetOperationalSafetyByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single OperationalSafety read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of
//                  the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.OperationalSafety.Models;

namespace PlantSafety.Application.OperationalSafety.Queries;

/// <summary>Retrieves a single OperationalSafety read-model record by identity.</summary>
public sealed record GetOperationalSafetyByIdQuery(
    Guid TenantId,
    Guid OperationalSafetyId) : IRequest<Result<OperationalSafetyItemDto>>;
