// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           GetGovernanceByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Governance read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of
//                  the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Governance.Models;

namespace PlantSafety.Application.Governance.Queries;

/// <summary>Retrieves a single Governance read-model record by identity.</summary>
public sealed record GetGovernanceByIdQuery(
    Guid TenantId,
    Guid GovernanceId) : IRequest<Result<GovernanceItemDto>>;
