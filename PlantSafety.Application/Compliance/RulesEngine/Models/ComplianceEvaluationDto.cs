// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a full ComplianceEvaluation run.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RulesEngine.Models;

/// <summary>Read-model DTO for a full ComplianceEvaluation run.</summary>
public sealed record ComplianceEvaluationDto
{
    /// <summary>Evaluation identity.</summary>
    public required Guid ComplianceEvaluationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the evaluated entity.</summary>
    public required string TargetEntityType { get; init; }

    /// <summary>Identity of the evaluated entity.</summary>
    public required Guid TargetEntityId { get; init; }

    /// <summary>Overall outcome (InProgress/Compliant/NonCompliant).</summary>
    public required string Status { get; init; }

    /// <summary>Rule results recorded during this run.</summary>
    public required IReadOnlyList<ComplianceRuleResultDto> RuleResults { get; init; }

    /// <summary>UTC instant the run started.</summary>
    public required DateTime StartedAtUtc { get; init; }

    /// <summary>UTC instant the run completed, if any.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or system actor who triggered the run.</summary>
    public required Guid TriggeredBy { get; init; }
}
