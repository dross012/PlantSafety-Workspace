// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           GetProjectByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Project read-model record by identity, including its
//                  current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries
//                  .GetRootCauseAnalysisByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Queries;

/// <summary>Retrieves a single Project read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ProjectId">Identity of the Project record to retrieve.</param>
public sealed record GetProjectByIdQuery(
    Guid TenantId,
    Guid ProjectId) : IRequest<Result<ProjectDetailDto>>;
