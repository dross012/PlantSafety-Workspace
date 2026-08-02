// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           GetAiSafetyIntelligenceByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single AiSafetyIntelligence read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AiSafetyIntelligence.Models;

namespace PlantSafety.Application.AiSafetyIntelligence.Queries;

/// <summary>Retrieves a single AiSafetyIntelligence read-model record by identity.</summary>
public sealed record GetAiSafetyIntelligenceByIdQuery(
    Guid TenantId,
    Guid AiSafetyIntelligenceId) : IRequest<Result<AiSafetyIntelligenceItemDto>>;
