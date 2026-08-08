// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance
//  Layer:          Application
//  File:           StandardsComplianceReportDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Application-layer projection of the Domain-layer
//                  PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects
//                  .StandardsComplianceReport record, returned to API callers by every
//                  EvaluateXComplianceCommand/EvaluateXStandardsCommand handler under
//                  Compliance.Standards.Compliance (CraneAndHoist, ControlSystemSafety,
//                  MachineSafety, RiskAssessment, OperationalSafety, PressureVessel). Mirrors
//                  Compliance.EPA.Compliance.Models.EpaComplianceReportDto exactly (same shape,
//                  same "no bleed across module boundaries" convention). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Standards.Compliance.Models;

/// <summary>
/// Result of evaluating a site/facility's operational data against a single ANSI/ASME/ISO
/// consensus-standards program (ASME B30, ISO 13849-1/IEC 62061, ANSI B11/ISO 12100 machine
/// safety, ISO 12100/ANSI B11.0 risk assessment, ANSI Z10/ISO 45001/A1264/Z117 operational
/// safety, or ASME BPVC pressure vessel).
/// </summary>
public sealed record StandardsComplianceReportDto
{
    /// <summary>Engine code identifying which consensus-standards engine was evaluated (e.g. "ASME_B30", "ISO_13849", "ANSI_B11_ISO_MACHINE_SAFETY").</summary>
    public required string EngineCode { get; init; }

    /// <summary>UTC instant the evaluation was performed.</summary>
    public required DateTime EvaluatedAtUtc { get; init; }

    /// <summary>Findings that represent an active compliance violation.</summary>
    public required IReadOnlyList<StandardsComplianceFindingDto> Violations { get; init; }

    /// <summary>Findings that represent an advisory or approaching-deadline condition, not yet a violation.</summary>
    public required IReadOnlyList<StandardsComplianceFindingDto> Warnings { get; init; }

    /// <summary>Recommended remediation/follow-up actions surfaced by the evaluation.</summary>
    public required IReadOnlyList<string> RequiredActions { get; init; }

    /// <summary><see langword="true"/> when no violations were found.</summary>
    public required bool IsCompliant { get; init; }
}
