// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance
//  Layer:          Application
//  File:           EpaComplianceReportDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Application-layer projection of the Domain-layer
//                  PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects.EpaComplianceReport
//                  record, returned to API callers by every EvaluateXComplianceCommand handler
//                  under Compliance.EPA.Compliance (RCRA, TSCA, CERCLA/SARA, Clean Air Act,
//                  Clean Water Act). Named EpaComplianceReportDto (not the bare "EPA..."
//                  spelling) to avoid collision with the pre-existing, unrelated bare-"EPA"
//                  identifiers used by Compliance.Certification.EPA and
//                  Compliance.RegulatoryCalendar.Deadlines.EPA. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.EPA.Compliance.Models;

/// <summary>
/// Result of evaluating a site/facility's operational data against a single EPA regulatory
/// program's requirements (RCRA, TSCA, CERCLA/SARA, Clean Air Act, or Clean Water Act).
/// </summary>
public sealed record EpaComplianceReportDto
{
    /// <summary>Program code identifying which EPA regulatory program was evaluated (e.g. "RCRA", "TSCA", "CERCLA_SARA", "CAA", "CWA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>UTC instant the evaluation was performed.</summary>
    public required DateTime EvaluatedAtUtc { get; init; }

    /// <summary>Findings that represent an active compliance violation.</summary>
    public required IReadOnlyList<EpaComplianceFindingDto> Violations { get; init; }

    /// <summary>Findings that represent an advisory or approaching-deadline condition, not yet a violation.</summary>
    public required IReadOnlyList<EpaComplianceFindingDto> Warnings { get; init; }

    /// <summary>Recommended remediation/follow-up actions surfaced by the evaluation.</summary>
    public required IReadOnlyList<string> RequiredActions { get; init; }

    /// <summary><see langword="true"/> when no violations were found.</summary>
    public required bool IsCompliant { get; init; }
}
