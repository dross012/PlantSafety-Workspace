// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetTaskComplexityProfileQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving a single task-complexity profile by task code. Backs the Task
//                  Complexity vs. Load Correlation dashboard. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;

/// <summary>Retrieves a single task-complexity profile by task code.</summary>
public sealed record GetTaskComplexityProfileQuery(
    Guid TenantId,
    string TaskCode) : IRequest<Result<TaskComplexityProfileDto>>;
