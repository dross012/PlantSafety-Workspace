// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the RiskManagement module's commands. Domain fields
//   only; envelope fields (CommandId/CorrelationId/TenantId/ActorId/etc.) are populated
//   server-side by RequestEnvelopeFactory.
// SchemaVersion: 1

namespace PlantSafety.Api.RiskManagement.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/riskmanagement</c>.</summary>
public sealed class CreateRiskRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable hazard title/summary.</summary>
    public required string RiskTitle { get; init; }

    /// <summary>Free-form location description (building/zone/equipment).</summary>
    public required string Location { get; init; }

    /// <summary>Hazard source classification (mechanical, electrical, chemical, ergonomic, process, human factors).</summary>
    public required string HazardSource { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public bool ReviewRequired { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/riskmanagement/{riskId}/score</c>.</summary>
public sealed class ScoreRiskRequest
{
    /// <summary>Severity component of the Severity x Likelihood matrix.</summary>
    public int Severity { get; init; }

    /// <summary>Likelihood component of the Severity x Likelihood matrix.</summary>
    public int Likelihood { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/riskmanagement/{riskId}/prioritize</c>.</summary>
public sealed class PrioritizeRiskRequest
{
    /// <summary>P1-P4 remediation priority.</summary>
    public string? Priority { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/riskmanagement/{riskId}/control-measures</c>.</summary>
public sealed class ApplyRiskControlMeasureRequest
{
    /// <summary>Hierarchy of Controls level (elimination, substitution, engineering, administrative, PPE).</summary>
    public required string HierarchyLevel { get; init; }

    /// <summary>Description of the applied control measure.</summary>
    public required string Description { get; init; }

    /// <summary>Optional justification for the selected hierarchy level.</summary>
    public string? Justification { get; init; }

    /// <summary>Identity of the person responsible for the control measure.</summary>
    public required Guid ResponsiblePersonId { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/riskmanagement/{riskId}/validate-effectiveness</c>.</summary>
public sealed class ValidateRiskEffectivenessRequest
{
    /// <summary>Residual severity after controls are applied.</summary>
    public int ResidualSeverity { get; init; }

    /// <summary>Residual likelihood after controls are applied.</summary>
    public int ResidualLikelihood { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/riskmanagement/{riskId}/complete-review</c>.</summary>
public sealed class CompleteRiskReviewRequest
{
    /// <summary>Optional new review cadence to apply going forward.</summary>
    public string? NewReviewCadence { get; init; }
}

/// <summary>Query-string filters shared by all RiskManagement category read-model endpoints.</summary>
public sealed class CategoryQueryRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the UTC date range.</summary>
    public DateTime From { get; init; }

    /// <summary>Inclusive end of the UTC date range.</summary>
    public DateTime To { get; init; }
}
