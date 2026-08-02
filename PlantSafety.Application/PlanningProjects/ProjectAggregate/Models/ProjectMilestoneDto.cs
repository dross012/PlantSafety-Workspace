// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           ProjectMilestoneDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model DTO for a single milestone belonging to a Project. Backs
//                  ProjectDetailDto.Milestones.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;

/// <summary>A single milestone belonging to a Project.</summary>
public sealed record ProjectMilestoneDto
{
    /// <summary>Milestone identifier.</summary>
    public required Guid MilestoneId { get; init; }

    /// <summary>Milestone name.</summary>
    public required string Name { get; init; }

    /// <summary>Target completion date (UTC).</summary>
    public required DateTime TargetDateUtc { get; init; }

    /// <summary>Indicates whether this milestone has been achieved.</summary>
    public required bool IsAchieved { get; init; }
}
