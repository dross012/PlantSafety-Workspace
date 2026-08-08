// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyStandardsInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input records consumed by OperationalSafetyStandardsEngine.EvaluateCompliance.
//                  This codebase has no real safety-management-system aggregate (ANSI Z10 / ISO
//                  45001 shaped) and no real walking/working-surface hazard log (ANSI A1264), so
//                  SafetyManagementSystemSnapshot and WalkingWorkingSurfaceHazardRecord are fully
//                  caller-supplied with no backing aggregate -- documented honestly, matching the
//                  EPA module's TSCA/CAA precedent for genuinely-absent domain data. Confined-space
//                  permit data DOES exist for real, under
//                  PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models.ConfinedSpaceItemDto
//                  -- ConfinedSpacePermitSnapshot is the projection shape this engine consumes; the
//                  calling Application-layer handler (built by a separate later effort) is
//                  responsible for projecting the real DTO into this snapshot. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.OperationalSafety.Services;

/// <summary>
/// Input to <see cref="OperationalSafetyStandardsEngine.EvaluateCompliance"/>.
/// </summary>
/// <param name="SafetyManagementSystem">
/// Structural safety-management-system snapshot for ANSI Z10 / ISO 45001 evaluation, or
/// <see langword="null"/> if not yet assessed. No real aggregate backs this in this codebase --
/// fully caller-supplied.
/// </param>
/// <param name="SurfaceHazards">
/// Walking/working-surface hazard records for ANSI A1264 evaluation. No real aggregate backs this
/// in this codebase -- fully caller-supplied.
/// </param>
/// <param name="ConfinedSpacePermits">
/// Confined-space entry permit snapshots for ANSI Z117 evaluation. REAL data, projected from
/// PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models.ConfinedSpaceItemDto by the
/// calling Application-layer handler.
/// </param>
/// <param name="EvaluatedAtUtc">When this compliance evaluation is being performed.</param>
public sealed record OperationalSafetyStandardsInput(
    SafetyManagementSystemSnapshot? SafetyManagementSystem,
    IReadOnlyList<WalkingWorkingSurfaceHazardRecord> SurfaceHazards,
    IReadOnlyList<ConfinedSpacePermitSnapshot> ConfinedSpacePermits,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Structural snapshot of an occupational health &amp; safety management system's PDCA (Plan-Do-
/// Check-Act) elements, per ANSI Z10 / ISO 45001. No real aggregate backs this in this codebase --
/// this is a caller-supplied projection of whatever source currently tracks management-system
/// maturity (e.g. a manual audit checklist) until a dedicated aggregate exists.
/// </summary>
/// <param name="HasDocumentedPolicy">Whether a documented occupational health &amp; safety policy exists (ANSI Z10 / ISO 45001 "Policy" element).</param>
/// <param name="HasPlanningProcess">Whether a hazard-identification/risk-assessment planning process exists ("Planning" element).</param>
/// <param name="HasImplementationSupport">Whether implementation and operational support (resources, competence, communication) exists ("Support"/"Operation" elements).</param>
/// <param name="HasPerformanceEvaluation">Whether performance monitoring/measurement/audit exists ("Performance Evaluation" element).</param>
/// <param name="HasContinualImprovementProcess">Whether a continual-improvement/management-review process exists ("Improvement" element).</param>
public sealed record SafetyManagementSystemSnapshot(
    bool HasDocumentedPolicy,
    bool HasPlanningProcess,
    bool HasImplementationSupport,
    bool HasPerformanceEvaluation,
    bool HasContinualImprovementProcess);

/// <summary>
/// A single walking/working-surface hazard record for ANSI A1264 evaluation. No real aggregate
/// backs this in this codebase -- fully caller-supplied.
/// </summary>
/// <param name="Location">Where the hazard was observed.</param>
/// <param name="HazardDescription">Qualitative description of the hazard/condition observed (e.g. "wet floor, no signage", "uncovered floor opening").</param>
/// <param name="Corrected">Whether the hazard has been corrected.</param>
public sealed record WalkingWorkingSurfaceHazardRecord(
    string Location,
    string HazardDescription,
    bool Corrected);

/// <summary>
/// Projection of a real <c>ConfinedSpaceItemDto</c> confined-space entry permit record, for ANSI
/// Z117 evaluation.
/// </summary>
/// <param name="PermitId">Permit identifier.</param>
/// <param name="SpaceIdentifier">Identifier of the confined space being entered.</param>
/// <param name="AttendantName">Name of the designated safety attendant, or null/blank if not assigned.</param>
/// <param name="Status">Current permit status (e.g. Active/Issued/Expired/Closed).</param>
/// <param name="IssuedAtUtc">When the permit was issued, if known.</param>
/// <param name="ExpiresAtUtc">When the permit expires, if known.</param>
public sealed record ConfinedSpacePermitSnapshot(
    Guid PermitId,
    string SpaceIdentifier,
    string? AttendantName,
    string Status,
    UtcTimestamp? IssuedAtUtc,
    UtcTimestamp? ExpiresAtUtc);
