// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           GetScoreCardsByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ScoreCards read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Queries;

/// <summary>Retrieves a single ScoreCards read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ScoreCardsId">Identity of the ScoreCards record to retrieve.</param>
public sealed record GetScoreCardsByIdQuery(
    Guid TenantId,
    Guid ScoreCardsId) : IRequest<Result<ScoreCardsItemDto>>;
