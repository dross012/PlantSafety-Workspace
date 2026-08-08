// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.RiskAssessment
//  Layer:          Domain
//  File:           RiskReductionHierarchyStep.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Simple classification enum (no metadata, no behavior) representing the three-step
//                  risk-reduction hierarchy defined by ISO 12100 (Safety of machinery -- General
//                  principles for design -- Risk assessment and risk reduction): (1) inherently safe
//                  design, (2) technical protective measures (guards, interlocks, light curtains,
//                  safety functions), (3) information for use (warnings, manuals, instructions for
//                  residual risk). ANSI B11.0-2023 harmonizes with this same methodology. Per
//                  Handbook §2.7, a simple (not Smart) enum is appropriate here -- pure classification
//                  vocabulary with no independent business rules or metadata beyond its ordering.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.RiskAssessment.Enums;

/// <summary>
/// The three-step ISO 12100 / ANSI B11.0 risk-reduction hierarchy. Designers/assessors must exhaust
/// each step, in order, before relying on the next.
/// </summary>
public enum RiskReductionHierarchyStep
{
    /// <summary>Step 1: eliminate or reduce hazards by design (e.g. eliminating pinch points, reducing energy).</summary>
    InherentlySafeDesign = 0,

    /// <summary>Step 2: guards, interlocks, light curtains, safety-rated control functions, and other engineered safeguards.</summary>
    TechnicalProtectiveMeasures = 1,

    /// <summary>Step 3: warnings, signage, training, and operating instructions addressing residual risk that cannot be designed or guarded out.</summary>
    InformationForUse = 2,
}
