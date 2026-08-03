// ============================================================================
// File: RouteDefinitionRequests.cs
// Project: PlantSafety.Api
// Module: ProcessManagement.Routing
// Created: 2026-08-03
// Description: Request DTOs bound from the HTTP request body/query string for the
//              ProcessManagement.Routing V1 controller. Covers the list/detail/steps/versions/
//              diff/simulation/approval reads exactly as consumed by
//              PlantSafety.UI.Modules.Routing.ApiClients.RoutingApiClient, plus the full
//              lifecycle: Create -> SubmitForReview -> Approve -> Retire -> Archive, and the
//              same-status Revise action. Mirrors
//              PlantSafety.Api.ProcessManagement.Contracts.Requests.ProcessDefinitionRequests.cs
//              -- every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.ProcessManagement.Routing.Contracts.Requests;

/// <summary>Query parameters for the paginated, optionally search-filtered routing list.</summary>
public sealed record ListRouteDefinitionsRequest
{
    /// <summary>1-based page number. Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Optional free-text search term matched against Name/PlantName.</summary>
    public string? Search { get; init; }
}

/// <summary>A single ordered step, as bound from a Create/Revise request body.</summary>
public sealed record RouteStepRequestItem
{
    /// <summary>Existing step identity to preserve across a Revise (null for a newly added step, or on Create).</summary>
    public Guid? ExistingStepId { get; init; }

    /// <summary>Step display name.</summary>
    public required string Name { get; init; }

    /// <summary>Step execution instructions.</summary>
    public required string Instructions { get; init; }

    /// <summary>Assigned resource names for this step.</summary>
    public IReadOnlyList<string> Resources { get; init; } = [];

    /// <summary>Estimated execution duration, in minutes.</summary>
    public int EstimatedDurationMinutes { get; init; }
}

/// <summary>Request body for submitting a new RouteDefinition record.</summary>
public sealed record CreateRouteDefinitionRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Plant/site display name.</summary>
    public required string PlantName { get; init; }

    /// <summary>Human-readable route display name.</summary>
    public required string Name { get; init; }

    /// <summary>Initial ordered step list. Must contain at least one step.</summary>
    public required IReadOnlyList<RouteStepRequestItem> Steps { get; init; }

    /// <summary>Identity of the accountable route owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Display name of the accountable route owner.</summary>
    public required string OwnerName { get; init; }

    /// <summary>Effective start date (UTC).</summary>
    public required DateTime EffectiveFromUtc { get; init; }

    /// <summary>Effective end date (UTC), if any.</summary>
    public DateTime? EffectiveToUtc { get; init; }
}

/// <summary>Request body for submitting a route definition for review (Draft -&gt; UnderReview).</summary>
public sealed record SubmitRouteDefinitionForReviewRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this route. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for approving a route definition under review (UnderReview -&gt; Active).</summary>
public sealed record ApproveRouteDefinitionRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this route. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for retiring an active route definition (Active -&gt; Retired).</summary>
public sealed record RetireRouteDefinitionRequest
{
    /// <summary>Optional narrative reason for retirement.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this route. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a retired route definition (Retired -&gt; Archived).</summary>
public sealed record ArchiveRouteDefinitionRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this route. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for revising an active route definition in place (replaces the ordered step list).</summary>
public sealed record ReviseRouteDefinitionRequest
{
    /// <summary>Replacement ordered step list. Must contain at least one step.</summary>
    public required IReadOnlyList<RouteStepRequestItem> Steps { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this route. Null skips the check.</summary>
    public string? ETag { get; init; }
}
