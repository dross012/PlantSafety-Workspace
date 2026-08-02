// ============================================================================
// File: PermitsRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Permits
// Created: 2026-07-30
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.Permits V1 controller. Covers the full Permit-to-Work lifecycle:
//              Create -> Approve/Reject -> Complete/Expire -> Close -> Archive. Mirrors
//              PlantSafety.Api.PermitToWork.Contracts.Requests.PermitToWorkRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. PermitsItemDto.ETag);
//              a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Contracts.Requests;

/// <summary>Request body for submitting a new Permits request.</summary>
public sealed record CreatePermitsRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    public required string PermitType { get; init; }

    /// <summary>Human-readable permit number/reference.</summary>
    public required string PermitNumber { get; init; }

    /// <summary>Description of the work covered by this permit.</summary>
    public required string WorkDescription { get; init; }

    /// <summary>Physical location the permit applies to.</summary>
    public required string Location { get; init; }

    /// <summary>Hazards identified for this permitted work.</summary>
    public required string HazardsIdentified { get; init; }
}

/// <summary>Request body for approving and issuing a Permits request (Created -&gt; Active).</summary>
public sealed record ApprovePermitsRequest
{
    /// <summary>Optional validity window, in hours, from approval. Null means no computed expiration.</summary>
    public int? ValidityHours { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for declining a Permits request (Created -&gt; Rejected).</summary>
public sealed record RejectPermitsRequest
{
    /// <summary>Reason for rejection.</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking the permitted work completed (Active -&gt; Completed).</summary>
public sealed record CompletePermitsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for expiring an issued permit that passed its validity window (Active -&gt; Expired).</summary>
public sealed record ExpirePermitsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed permit (Completed -&gt; Closed).</summary>
public sealed record ClosePermitsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed permit for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchivePermitsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Permits record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the Permits read-model list endpoint.</summary>
public sealed record PermitsListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
