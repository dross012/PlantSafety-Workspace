// ============================================================================
// File: PermitToWorkRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              PermitToWork V1 controller (PermitWorkflow, PermitTemplate, PermitScheduling,
//              PermitExpiration). Covers the full PTW lifecycle: Request -> AssessHazards ->
//              ImplementControls -> Approve -> Issue -> RecordClosureSignature -> Archive,
//              plus Reject.
// ============================================================================

namespace PlantSafety.Api.PermitToWork.Contracts.Requests;

/// <summary>Request body for submitting a new Permit-to-Work request (PTW Workflow Step 1).</summary>
public sealed record RequestPermitWorkflowRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable permit number/identifier.</summary>
    public required string PermitNumber { get; init; }

    /// <summary>Permit category code (e.g. HOT-WORK, CONFINED-SPACE, ELECTRICAL-ISOLATION, WORKING-AT-HEIGHT, EXCAVATION).</summary>
    public required string PermitTypeCode { get; init; }

    /// <summary>Scope-of-work description: task, exact location, equipment involved, expected duration.</summary>
    public required string WorkDescription { get; init; }
}

/// <summary>Request body for recording the hazard assessment and required controls (PTW Workflow Step 2).</summary>
public sealed record AssessPermitHazardsRequest
{
    /// <summary>Required controls identified (LOTO, PPE, ventilation, fire watch, barricades, atmospheric testing).</summary>
    public required string RequiredControls { get; init; }

    /// <summary>Risk score assigned by the safety officer.</summary>
    public required int RiskScore { get; init; }
}

/// <summary>Request body for recording that required safety controls have been implemented (PTW Workflow Step 3).</summary>
public sealed record ImplementPermitControlsRequest
{
    /// <summary>Identity/role of the person confirming the controls are in place.</summary>
    public required string ControlsConfirmedBy { get; init; }
}

/// <summary>Request body for issuing an approved permit, making it active (PTW Workflow Step 4/5).</summary>
public sealed record IssuePermitWorkflowRequest
{
    /// <summary>Optional validity window, in hours, from issuance. Null means no computed expiration.</summary>
    public int? ValidityHours { get; init; }
}

/// <summary>Request body for recording the closure signature for an active permit (PTW Workflow Step 6).</summary>
public sealed record RecordPermitClosureSignatureRequest
{
    /// <summary>Optional closure notes from the post-work review.</summary>
    public string? ClosureNotes { get; init; }
}

/// <summary>Request body for declining a PermitWorkflow request.</summary>
public sealed record RejectPermitWorkflowRequest
{
    /// <summary>Reason for rejection.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for deprecating an active PermitTemplate.</summary>
public sealed record DeprecatePermitTemplateRequest
{
    /// <summary>Reason the template is being deprecated.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for a bulk PermitToWork operation.</summary>
public sealed record BulkUpdatePermitToWorkRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifiers of the PermitToWork resources to operate on. Maximum 500 per request.</summary>
    public required IReadOnlyList<Guid> ResourceIds { get; init; }

    /// <summary>The operation to apply (e.g. "Close", "Approve", "Reassign").</summary>
    public required string Operation { get; init; }

    /// <summary>Optional operation-specific key-value data.</summary>
    public Dictionary<string, string>? Parameters { get; init; }
}

/// <summary>Query-string bound filters for the PermitToWork read-model list endpoints.</summary>
public sealed record PermitToWorkListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
