// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           GetManagementOfChangeByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ManagementOfChange read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.ManagementOfChange.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Queries;

/// <summary>Retrieves a single ManagementOfChange read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ManagementOfChangeId">Identity of the ManagementOfChange record to retrieve.</param>
public sealed record GetManagementOfChangeByIdQuery(
    Guid TenantId,
    Guid ManagementOfChangeId) : IRequest<Result<ManagementOfChangeItemDto>>;
