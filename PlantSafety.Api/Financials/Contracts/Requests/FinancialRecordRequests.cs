// ============================================================================
// File: FinancialRecordRequests.cs
// Project: PlantSafety.Api
// Module: Financials
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the new FinancialRecord V1
//              controller. Covers the full lifecycle: Create -> AddLineItem -> Submit -> Approve
//              / Reject -> Close -> Archive. Distinct from the pre-existing, unrelated
//              PlantSafety.Api.Financials.Contracts.Requests.GetFinancialsRequest. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Financials.Contracts.Requests;

/// <summary>Request body for creating a new FinancialRecord (Status=Draft).</summary>
public sealed record CreateFinancialRecordRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Financial category (e.g. Budget, Cost, Forecast).</summary>
    public required string Category { get; init; }

    /// <summary>Monetary amount.</summary>
    public required decimal Amount { get; init; }
}

/// <summary>Request body for appending a new line item to a Draft FinancialRecord.</summary>
public sealed record AddFinancialRecordLineItemRequest
{
    /// <summary>Description of the line item.</summary>
    public required string Description { get; init; }

    /// <summary>Amount in the site's base currency.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting a FinancialRecord for approval (Draft -&gt; Submitted).</summary>
public sealed record SubmitFinancialRecordRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for approving a FinancialRecord (Submitted -&gt; Approved).</summary>
public sealed record ApproveFinancialRecordRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for rejecting a FinancialRecord (Submitted -&gt; Rejected).</summary>
public sealed record RejectFinancialRecordRequest
{
    /// <summary>Optional narrative reason for the rejection.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a FinancialRecord (Approved -&gt; Closed).</summary>
public sealed record CloseFinancialRecordRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a FinancialRecord (Closed or Rejected -&gt; Archived).</summary>
public sealed record ArchiveFinancialRecordRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
