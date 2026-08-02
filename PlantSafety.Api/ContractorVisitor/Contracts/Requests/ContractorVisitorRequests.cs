// ============================================================================
// File: ContractorVisitorRequests.cs
// Project: PlantSafety.Api
// Module: ContractorVisitor
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the ContractorVisitor V1
//              controller. Covers the full lifecycle: Create -> Activate -> CheckIn -> Complete ->
//              Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.ContractorVisitor.Contracts.Requests;

/// <summary>Request body for registering a new ContractorVisitor site-access record.</summary>
public sealed record CreateContractorVisitorRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Full name of the contractor or visitor.</summary>
    public required string FullName { get; init; }

    /// <summary>Company name the contractor or visitor represents.</summary>
    public required string CompanyName { get; init; }

    /// <summary>Visitor classification (Contractor/Visitor).</summary>
    public string VisitorType { get; init; } = "Visitor";

    /// <summary>Narrative purpose of the visit.</summary>
    public required string PurposeOfVisit { get; init; }

    /// <summary>Identity of the host user sponsoring this visit.</summary>
    public required Guid HostUserId { get; init; }

    /// <summary>Optional scheduled arrival timestamp.</summary>
    public DateTime? ScheduledArrivalUtc { get; init; }
}

/// <summary>Request body for activating a ContractorVisitor record -- approved/scheduled (Created -&gt; Active).</summary>
public sealed record ActivateContractorVisitorRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording a contractor/visitor's actual arrival (same-status action while Active).</summary>
public sealed record CheckInContractorVisitorRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a ContractorVisitor record (Active -&gt; Completed).</summary>
public sealed record CompleteContractorVisitorRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a ContractorVisitor record (Completed -&gt; Closed).</summary>
public sealed record CloseContractorVisitorRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a ContractorVisitor record (Closed -&gt; Archived).</summary>
public sealed record ArchiveContractorVisitorRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
