// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           GetComplianceByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single (self-named) Compliance read-model record by
//                  identity, including its current ETag, so a caller can read-before-write into
//                  one of the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Compliance.Models;

namespace PlantSafety.Application.Compliance.Compliance.Queries;

/// <summary>Retrieves a single Compliance read-model record by identity.</summary>
public sealed record GetComplianceByIdQuery(
    Guid TenantId,
    Guid ComplianceId) : IRequest<Result<ComplianceItemDto>>;
