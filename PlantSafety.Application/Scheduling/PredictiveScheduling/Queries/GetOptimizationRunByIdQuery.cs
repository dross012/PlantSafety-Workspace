// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           GetOptimizationRunByIdQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single OptimizationRun read-model record by identity.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Queries;

/// <summary>Retrieves a single OptimizationRun read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="OptimizationRunId">Identity of the OptimizationRun record to retrieve.</param>
public sealed record GetOptimizationRunByIdQuery(
    Guid TenantId,
    Guid OptimizationRunId) : IRequest<Result<OptimizationRunDto>>;
