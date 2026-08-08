// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Shared
//  Layer:          Domain
//  File:           StandardsComplianceReport.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Shared consensus-standards (ANSI/ASME/ISO) compliance-evaluation report shape,
//                  common to every engine under Compliance.Standards (RiskAssessment,
//                  OperationalSafety, PressureVessel, and -- built by a parallel effort --
//                  CraneAndHoist, ControlSystemSafety, MachineSafety). Defined once here so all
//                  Compliance.Standards engines return a single, consistent type rather than each
//                  declaring a duplicate/near-duplicate shape (mirrors the precedent set by
//                  Compliance.EPA.Shared.ValueObjects.EpaComplianceReport for the EPA program
//                  family). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;

/// <summary>Uniform compliance-evaluation result shared by every consensus-standards rule engine (ANSI/ASME/ISO).</summary>
public sealed record StandardsComplianceReport(
    string EngineCode,
    PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp EvaluatedAtUtc,
    IReadOnlyList<StandardsComplianceFinding> Violations,
    IReadOnlyList<StandardsComplianceFinding> Warnings,
    IReadOnlyList<string> RequiredActions,
    bool IsCompliant);

/// <summary>A single compliance finding (violation, warning, or advisory) within a <see cref="StandardsComplianceReport"/>.</summary>
public sealed record StandardsComplianceFinding(
    string Citation,
    string Description,
    string Severity,
    string? RelatedEntityId);
