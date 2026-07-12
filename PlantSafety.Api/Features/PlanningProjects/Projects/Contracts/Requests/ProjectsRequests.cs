// ============================================================================
// File: ProjectsRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Request DTOs bound from the HTTP request body for the
//              PlanningProjects/Projects Features/ controller.
// ============================================================================

namespace PlantSafety.Api.Features.PlanningProjects.Projects.Contracts.Requests;

/// <summary>Request body for creating a new Safety Project.</summary>
public sealed record CreateSafetyProjectRequest
{
    /// <summary>Primary site for this project.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Project name. Max 200 characters.</summary>
    public required string Name { get; init; }

    /// <summary>Description of scope and objectives. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Project owner.</summary>
    public required Guid ProjectOwnerId { get; init; }

    /// <summary>Project manager, if assigned at creation.</summary>
    public Guid? ProjectManagerId { get; init; }

    /// <summary>Planned start date (UTC).</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Planned completion date (UTC).</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }

    /// <summary>Planned budget, if known.</summary>
    public decimal? PlannedBudget { get; init; }
}
