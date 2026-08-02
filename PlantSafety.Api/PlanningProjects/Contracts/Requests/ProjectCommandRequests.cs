// ============================================================================
// File: ProjectCommandRequests.cs
// Project: PlantSafety.Api
// Module: PlanningProjects.ProjectAggregate
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the
//              PlanningProjects.ProjectAggregate V1 controller's Phase 4 full build-out lifecycle
//              actions (Create -> Activate -> PutOnHold/Resume -> Complete/Cancel -> Archive, plus
//              AddMilestone/AchieveMilestone). Every mutating lifecycle request carries an optional
//              ETag field so clients can supply the optimistic-concurrency token captured from a
//              prior read (e.g. ProjectDetailDto.ETag); a stale value results in a
//              concurrency-conflict error. Mirrors
//              PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Requests
//              .RootCauseAnalysisCommandRequests.
// ============================================================================

namespace PlantSafety.Api.PlanningProjects.Contracts.Requests;

/// <summary>Request body for creating a new Project.</summary>
public sealed record CreateProjectRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Project name.</summary>
    public required string Name { get; init; }

    /// <summary>UTC timestamp the project is targeted to complete.</summary>
    public required DateTime TargetCompletionUtc { get; init; }
}

/// <summary>Request body for activating a Project (Planning -&gt; Active).</summary>
public sealed record ActivateProjectRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for putting a Project on hold (Active -&gt; OnHold).</summary>
public sealed record PutProjectOnHoldRequest
{
    /// <summary>Reason the project is being put on hold.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for resuming a Project from hold (OnHold -&gt; Active).</summary>
public sealed record ResumeProjectRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a Project (Active -&gt; Completed).</summary>
public sealed record CompleteProjectRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for cancelling a Project (Planning/Active/OnHold -&gt; Cancelled).</summary>
public sealed record CancelProjectRequest
{
    /// <summary>Reason the project is being cancelled.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a Project (Completed/Cancelled -&gt; Archived).</summary>
public sealed record ArchiveProjectRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for adding a milestone to a Project.</summary>
public sealed record AddProjectMilestoneRequest
{
    /// <summary>Milestone name.</summary>
    public required string Name { get; init; }

    /// <summary>Target completion date (UTC).</summary>
    public required DateTime TargetDateUtc { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a Project milestone as achieved.</summary>
public sealed record AchieveProjectMilestoneRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
