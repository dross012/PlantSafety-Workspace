// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the AccidentManagement module's commands. Domain fields
//   only; envelope fields (CommandId/CorrelationId/TenantId/ActorId/etc.) are populated
//   server-side by RequestEnvelopeFactory.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- All mutating request DTOs gain an optional ETag (Phase 4
//   AccidentManagement concurrency remediation). CloseAccidentInvestigationRequest is new -- that
//   endpoint previously took no request body at all.

namespace PlantSafety.Api.AccidentManagement.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/accidentmanagement</c>.</summary>
public sealed class CreateAccidentRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable summary of the accident.</summary>
    public required string AccidentTitle { get; init; }

    /// <summary>Free-form location description (building/zone/machine).</summary>
    public required string Location { get; init; }

    /// <summary>AccidentSeverity code.</summary>
    public required string Severity { get; init; }

    /// <summary>Count of individuals injured.</summary>
    public int InjuredPersonCount { get; init; }

    /// <summary>UTC instant the accident actually occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>InvestigationPhase code governing this record.</summary>
    public required string Category { get; init; }

    /// <summary>Identity of the accountable owner (reporting supervisor/safety manager).</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public bool ReviewRequired { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/assign-investigator</c>.</summary>
public sealed class AssignAccidentInvestigatorRequest
{
    /// <summary>Identity of the investigator being assigned.</summary>
    public required Guid InvestigatorId { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/root-cause</c>.</summary>
public sealed class RecordAccidentRootCauseRequest
{
    /// <summary>Root cause classification (RCA/FMEA).</summary>
    public required string RootCauseCategory { get; init; }

    /// <summary>Free-form investigation notes.</summary>
    public required string Notes { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/corrective-actions</c>.</summary>
public sealed class CreateAccidentCorrectiveActionRequest
{
    /// <summary>Corrective action category.</summary>
    public required string Category { get; init; }

    /// <summary>Description of the corrective action.</summary>
    public required string Description { get; init; }

    /// <summary>Identity of the person responsible for completing the action.</summary>
    public required Guid ResponsiblePersonId { get; init; }

    /// <summary>UTC due date for completion.</summary>
    public required DateTime DueAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/corrective-actions/{correctiveActionId}/complete</c>.</summary>
public sealed class CompleteAccidentCorrectiveActionRequest
{
    /// <summary>Evidence supporting completion of the corrective action.</summary>
    public required string CompletionEvidence { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/regulatory-report</c>.</summary>
public sealed class FileAccidentRegulatoryReportRequest
{
    /// <summary>Regulatory agency the report was filed with (e.g. OSHA).</summary>
    public required string RegulatoryAgency { get; init; }

    /// <summary>Agency-assigned reference number for the filing.</summary>
    public required string ReferenceNumber { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/accidentmanagement/{accidentId}/close</c>.</summary>
public sealed class CloseAccidentInvestigationRequest
{
    /// <summary>Optimistic-concurrency token from a prior read. A stale value returns 409 Conflict.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string filters shared by all AccidentManagement category read-model endpoints.</summary>
public sealed class CategoryQueryRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the UTC date range.</summary>
    public DateTime From { get; init; }

    /// <summary>Inclusive end of the UTC date range.</summary>
    public DateTime To { get; init; }
}
