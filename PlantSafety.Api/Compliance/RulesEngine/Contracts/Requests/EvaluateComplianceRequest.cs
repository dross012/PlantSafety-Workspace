// ============================================================================
// File: EvaluateComplianceRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: External-facing request DTO for the Compliance.RulesEngine module's evaluate
//              endpoint. Mirrors PlantSafety.UI.Modules.Compliance.Models.EvaluateComplianceRequest
//              field-for-field so the UI's typed client can round-trip without translation.
// ============================================================================

namespace PlantSafety.Api.Compliance.RulesEngine.Contracts.Requests;

/// <summary>Request body for running the Compliance Rules Engine against a target entity.</summary>
public sealed record EvaluateComplianceRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the entity being evaluated (e.g. "Risk", "Permit", "AuditFinding").</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity being evaluated.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Optional subset of rule categories to run ("Safety"/"Regulatory"/"InternalPolicy"). Null runs all three.</summary>
    public IReadOnlyCollection<string>? Categories { get; init; }

    /// <summary>Caller-gathered, already-resolved data points the rule pipeline needs.</summary>
    public IReadOnlyDictionary<string, string?> Facts { get; init; } = new Dictionary<string, string?>();
}
