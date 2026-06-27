// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectMilestone.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Entity representing a milestone within a planning project.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Entities;

/// <summary>Represents a milestone within a planning/capital project.</summary>
public sealed class ProjectMilestone
{
    /// <summary>Unique identity of this milestone.</summary>
    public Guid MilestoneId { get; private set; }
    /// <summary>Name of the milestone.</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>Target completion date (UTC).</summary>
    public UtcTimestamp TargetDateUtc { get; private set; }
    /// <summary>Indicates whether this milestone has been achieved.</summary>
    public bool IsAchieved { get; private set; }
    /// <summary>For ORM rehydration only.</summary>
    private ProjectMilestone() { }
    /// <summary>Creates a new project milestone.</summary>
    public ProjectMilestone(Guid milestoneId, string name, UtcTimestamp targetDateUtc)
    {
        MilestoneId  = milestoneId;
        Name         = name;
        TargetDateUtc = targetDateUtc;
    }
    /// <summary>Marks the milestone as achieved.</summary>
    public void Achieve() => IsAchieved = true;
}
