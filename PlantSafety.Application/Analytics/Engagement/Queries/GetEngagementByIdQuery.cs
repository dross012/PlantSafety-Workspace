// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           GetEngagementByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Engagement read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Engagement.Models;

namespace PlantSafety.Application.Analytics.Engagement.Queries;

/// <summary>Retrieves a single Engagement read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EngagementId">Identity of the Engagement record to retrieve.</param>
public sealed record GetEngagementByIdQuery(
    Guid TenantId,
    Guid EngagementId) : IRequest<Result<EngagementItemDto>>;
