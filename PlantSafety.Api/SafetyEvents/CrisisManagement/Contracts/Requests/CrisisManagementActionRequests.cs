// ============================================================================
// File: CrisisManagementActionRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.CrisisManagement
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.CrisisManagement
//              Actions V1 controller. Covers the full CrisisManagement lifecycle: Declare ->
//              RespondTo -> Stabilize -> Resolve -> Close -> Archive. Mirrors
//              PlantSafety.Api.Operational.PredictiveMaintenance.Contracts.Requests
//              .PredictiveMaintenanceAlertRequests.cs -- every mutating lifecycle request carries
//              an optional ETag field so clients can supply the optimistic-concurrency token
//              captured from a prior read (e.g. CrisisManagementDetailDto.ETag); a stale value
//              results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.CrisisManagement.Contracts.Requests;

/// <summary>Request body for declaring a new CrisisManagement crisis.</summary>
public sealed record DeclareCrisisManagementRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short crisis title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>UTC instant the crisis was declared.</summary>
    public required DateTime DeclaredAtUtc { get; init; }
}

/// <summary>Request body for lifecycle transitions that only require an optional concurrency token.</summary>
public sealed record CrisisManagementLifecycleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this crisis. Null skips the check.</summary>
    public string? ETag { get; init; }
}
