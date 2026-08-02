// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Project lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Exceptions;

/// <summary>Thrown when a Project lifecycle transition is not permitted.</summary>
public sealed class ProjectInvalidStatusTransitionException : ProjectDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ProjectInvalidStatusTransitionException(
        ProjectId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition Project '{id}' from '{from}' to '{to}'.",
            "Project.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
