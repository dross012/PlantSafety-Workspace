// ============================================================================
// File: ComplianceRuleRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Compliance
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the (self-named)
//              Compliance.Compliance V1 controller. Covers the full lifecycle: Create -> Activate
//              -> Evaluate -> Complete -> Close -> Archive. File named "ComplianceRuleRequests"
//              (not bare "ComplianceRequests") for the same collision-avoidance reason documented
//              on the Infrastructure-layer ComplianceRuleAggregateDapperRepository -- "Compliance"
//              is heavily overloaded throughout this codebase. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Compliance.Compliance.Contracts.Requests;

/// <summary>Request body for submitting a new Compliance evaluation record.</summary>
public sealed record CreateComplianceRuleRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Regulation/standard reference this record evaluates.</summary>
    public required string RegulationReference { get; init; }

    /// <summary>Narrative description of the rule being evaluated.</summary>
    public required string RuleDescription { get; init; }

    /// <summary>Initial compliance score (0-100).</summary>
    public decimal ComplianceScore { get; init; }

    /// <summary>Initial count of open non-conformances.</summary>
    public int NonConformanceCount { get; init; }

    /// <summary>Identity of the party accountable for maintaining compliance.</summary>
    public required Guid ResponsiblePartyId { get; init; }
}

/// <summary>Request body for activating a Compliance evaluation record (Created -&gt; Active).</summary>
public sealed record ActivateComplianceRuleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Compliance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for re-evaluating an active Compliance record (status unchanged).</summary>
public sealed record EvaluateComplianceRuleRequest
{
    /// <summary>Updated compliance score (0-100).</summary>
    public required decimal ComplianceScore { get; init; }

    /// <summary>Updated count of open non-conformances.</summary>
    public required int NonConformanceCount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Compliance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a Compliance evaluation record completed (Active -&gt; Completed).</summary>
public sealed record CompleteComplianceRuleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Compliance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed Compliance record (Completed -&gt; Closed).</summary>
public sealed record CloseComplianceRuleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Compliance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Compliance record for retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveComplianceRuleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Compliance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
