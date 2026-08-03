// ============================================================================
// File: PredictiveSchedulingRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-03
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Scheduling.PredictiveScheduling V1 controller (OptimizationRun request/history).
// ============================================================================

using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

namespace PlantSafety.Api.Scheduling.Contracts.Requests;

/// <summary>Request body for running the predictive scheduling optimization pipeline.</summary>
public sealed record RequestOptimizationRunRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Candidate tasks competing for placement in this run.</summary>
    public required IReadOnlyList<SchedulableTaskInputDto> Tasks { get; init; }

    /// <summary>Declared resource availability windows for this run.</summary>
    public required IReadOnlyList<ResourceAvailabilityWindowInputDto> Availability { get; init; }

    /// <summary>Name of the OptimizationObjective value to optimize for (e.g. "MaximizeCoverage").</summary>
    public required string ObjectiveCode { get; init; }
}
