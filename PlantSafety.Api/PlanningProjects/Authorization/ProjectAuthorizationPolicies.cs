// ============================================================================
// File: ProjectAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Module: PlanningProjects.ProjectAggregate
// Created: 2026-08-02
// Description: Authorization policy name constants for the PlanningProjects.ProjectAggregate
//              module (the new DDD write-side aggregate). Named "ProjectAuthorizationPolicies"
//              (not "PlanningProjectsAuthorizationPolicies") to avoid colliding with the
//              pre-existing PlanningProjectsAuthorizationPolicies class backing the read-only
//              list-query vertical.
// ============================================================================

namespace PlantSafety.Api.PlanningProjects.Authorization;

/// <summary>Authorization policy name constants for the Project aggregate module.</summary>
public static class ProjectAuthorizationPolicies
{
    /// <summary>Read access to Project data. Granted to authorized roles.</summary>
    public const string Read = "RequirePlanningProjectsProjectRead";

    /// <summary>Write access to Project data. Granted to administrative roles.</summary>
    public const string Write = "RequirePlanningProjectsProjectWrite";
}
