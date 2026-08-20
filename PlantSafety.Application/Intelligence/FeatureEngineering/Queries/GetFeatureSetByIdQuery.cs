// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           GetFeatureSetByIdQuery.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single FeatureSet read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into AddFeatureToFeatureSet
//                  or PublishFeatureSet. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Queries.GetDataLakeActivityByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.FeatureEngineering.Models;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Queries;

/// <summary>Retrieves a single FeatureSet read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="FeatureSetId">Identity of the feature set to retrieve.</param>
public sealed record GetFeatureSetByIdQuery(
    Guid TenantId,
    Guid FeatureSetId) : IRequest<Result<FeatureSetDetailDto>>;
