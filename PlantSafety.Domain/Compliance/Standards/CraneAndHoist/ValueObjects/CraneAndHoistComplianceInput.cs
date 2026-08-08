// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.CraneAndHoist
//  Layer:          Domain
//  File:           CraneAndHoistComplianceInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input snapshot records accepted by CraneAndHoistComplianceEngine. Callers
//                  project the real ICraneHoistSystemsRepository bulk-query results (the
//                  PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.
//                  CraneHoistSystemsItemDto read model, plus any supplementary rigging/overload/
//                  operator-qualification detail the Application layer sources from related
//                  aggregates) into these records. No infrastructure/SQL/HTTP dependency here --
//                  Domain layer, pure/stateless. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.CraneAndHoist.ValueObjects;

/// <summary>Input to <see cref="Services.CraneAndHoistComplianceEngine.EvaluateCompliance"/>: a site's registered crane/hoist assets as of a point in time.</summary>
public sealed record CraneAndHoistComplianceInput(
    IReadOnlyList<CraneHoistAssetSnapshot> Assets,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Point-in-time snapshot of a single crane/hoist asset's ASME B30 compliance-relevant state,
/// projected from the real CraneHoistSystemsItemDto read model (AssetTag, AssetTitle, AssetType,
/// RatedLoadCapacityKg, LastObservedLoadKg, OverloadEventCount, InspectionDueAtUtc,
/// OperatorQualificationExpiresAtUtc) plus supplementary inspection/rigging/operator-qualification
/// detail (LastInspectionPassed, OperatorQualificationSuspended, rigging inspection fields) that
/// the Application-layer handler is expected to source from the CraneHoistSystems aggregate's
/// related inspection/qualification records when this engine is wired up.
/// </summary>
public sealed record CraneHoistAssetSnapshot(
    string AssetTag,
    string AssetTitle,
    string AssetType,
    decimal RatedLoadCapacityKg,
    decimal? LastObservedLoadKg,
    UtcTimestamp? LastLiftAtUtc,
    int OverloadEventCount,
    bool OverloadFlagged,
    UtcTimestamp? InspectionDueAtUtc,
    UtcTimestamp? LastInspectionAtUtc,
    bool? LastInspectionPassed,
    UtcTimestamp? OperatorQualifiedAtUtc,
    UtcTimestamp? OperatorQualificationExpiresAtUtc,
    bool OperatorQualificationSuspended,
    UtcTimestamp? LastRiggingInspectionAtUtc,
    bool LastRiggingDefectDetected,
    string Status);
