// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a single compliance violation detail. DTO-only per
//   Handbook §2.16F -- never returns domain types across the Application/API boundary.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RulesEngine.Models;

/// <summary>Read-model DTO for a single compliance violation detail.</summary>
public sealed record ComplianceViolationDto
{
    /// <summary>Stable machine-readable violation code.</summary>
    public required string ViolationCode { get; init; }

    /// <summary>Human-readable description.</summary>
    public required string Description { get; init; }

    /// <summary>Severity code (Advisory/Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Optional remediation guidance.</summary>
    public string? RemediationGuidance { get; init; }
}
