// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Shared
//  Layer:          Domain
//  File:           EpaComplianceReport.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Shared EPA compliance-evaluation report shape, common to every EPA program
//                  engine under Compliance.EPA (RCRA, CERCLA/SARA, and -- built by a parallel
//                  effort -- TSCA/CAA/CWA). Defined once here so all EPA program engines return
//                  a single, consistent type rather than each declaring a duplicate/near-duplicate
//                  shape. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

/// <summary>
/// Result of evaluating a site/facility's operational data against a single EPA regulatory
/// program's requirements. Returned by every engine under Compliance.EPA.
/// </summary>
public sealed record EpaComplianceReport(
    string ProgramCode,
    UtcTimestamp EvaluatedAtUtc,
    IReadOnlyList<EpaComplianceFinding> Violations,
    IReadOnlyList<EpaComplianceFinding> Warnings,
    IReadOnlyList<string> RequiredActions,
    bool IsCompliant);

/// <summary>A single compliance finding (violation, warning, or advisory) within an <see cref="EpaComplianceReport"/>.</summary>
public sealed record EpaComplianceFinding(
    string Citation,
    string Description,
    string Severity,
    string? RelatedEntityId);
