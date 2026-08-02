// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           GetCrisisManagementByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single CrisisManagement read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;

/// <summary>Retrieves a single CrisisManagement read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CrisisManagementId">Identity of the CrisisManagement record to retrieve.</param>
public sealed record GetCrisisManagementByIdQuery(
    Guid TenantId,
    Guid CrisisManagementId) : IRequest<Result<CrisisManagementDetailDto>>;
