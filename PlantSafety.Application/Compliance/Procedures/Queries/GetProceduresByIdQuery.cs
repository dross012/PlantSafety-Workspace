// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           GetProceduresByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Procedures read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.Models;

namespace PlantSafety.Application.Compliance.Procedures.Queries;

/// <summary>Retrieves a single Procedures read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ProceduresId">Identity of the Procedures record to retrieve.</param>
public sealed record GetProceduresByIdQuery(
    Guid TenantId,
    Guid ProceduresId) : IRequest<Result<ProceduresItemDto>>;
