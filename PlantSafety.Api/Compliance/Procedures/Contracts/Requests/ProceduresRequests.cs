// ============================================================================
// File: ProceduresRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Procedures
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Compliance.Procedures V1
//              controller. Covers the full lifecycle: Create -> Approve -> Supersede -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. ProceduresItemDto.ETag);
//              a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.Contracts.Requests;

/// <summary>Request body for submitting a new Procedures document.</summary>
public sealed record CreateProceduresRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable procedure title (e.g., "Confined Space Entry SOP").</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Procedure document number/reference.</summary>
    public required string ProcedureNumber { get; init; }

    /// <summary>Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).</summary>
    public required string Category { get; init; }
}

/// <summary>Request body for approving a Procedures document and making it effective (Created -&gt; Active).</summary>
public sealed record ApproveProceduresRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for superseding an active Procedures document with a newer revision (Active -&gt; Completed).</summary>
public sealed record SupersedeProceduresRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a superseded Procedures document (Completed -&gt; Closed).</summary>
public sealed record CloseProceduresRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Procedures document for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveProceduresRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
