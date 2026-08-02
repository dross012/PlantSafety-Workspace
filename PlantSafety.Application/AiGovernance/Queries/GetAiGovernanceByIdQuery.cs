// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           GetAiGovernanceByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single AiGovernance read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AiGovernance.Models;

namespace PlantSafety.Application.AiGovernance.Queries;

/// <summary>Retrieves a single AiGovernance read-model record by identity.</summary>
public sealed record GetAiGovernanceByIdQuery(
    Guid TenantId,
    Guid AiGovernanceId) : IRequest<Result<AiGovernanceItemDto>>;
