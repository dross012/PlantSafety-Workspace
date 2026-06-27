// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectStatus.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status for planning projects.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Enums;

/// <summary>Lifecycle states for planning/capital-project records.</summary>
public enum ProjectStatus
{
    /// <summary>Project is in the proposal/planning stage.</summary>
    Planning = 0,
    /// <summary>Project has been approved and is active.</summary>
    Active = 1,
    /// <summary>Project is on hold pending resource or approval.</summary>
    OnHold = 2,
    /// <summary>Project has been completed.</summary>
    Completed = 3,
    /// <summary>Project has been cancelled.</summary>
    Cancelled = 4,
    /// <summary>Project has been archived.</summary>
    Archived = 5,
}
