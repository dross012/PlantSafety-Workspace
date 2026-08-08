// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.CraneAndHoist
//  Layer:          Domain
//  File:           CraneAndHoistComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates ASME B30
//                  (crane/hoist/lifting-device safety) compliance for a site's registered crane
//                  and hoist assets. There is no persisted aggregate for this engine -- it is a
//                  pure calculation over caller-supplied snapshots projected from the real
//                  ICraneHoistSystemsRepository read model (same "stateless calculation service"
//                  pattern as Compliance.EPA.Rcra.Services.RcraWasteComplianceEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - ASME B30 defines three inspection intervals for cranes/hoists/lifting
//                      devices: Initial (before first use of new/altered/repaired equipment),
//                      Frequent (daily-to-monthly, high-wear items, by the operator/user), and
//                      Periodic (every 1-12 months, deeper documented inspection by a
//                      "qualified"/"designated" person). A designated person must verify
//                      compliance before initial use of new/altered/modified equipment.
//                    - ASME B30.10 (hooks): requires regular inspection for wear, deformation,
//                      and cracks. ASME B30.2/B30.9 (overhead cranes / slings): rated load
//                      capacity and rigging inspection requirements.
//                    - Overloading a crane/hoist beyond its rated load capacity is a fundamental
//                      ASME B30 violation regardless of which specific B30 sub-standard (B30.2
//                      overhead cranes, B30.9 slings, B30.16 hoists, B30.17 jib cranes, B30.5
//                      mobile cranes) applies to the specific asset type.
//                    - Only a currently-qualified/designated operator may operate B30-covered
//                      lifting equipment; a suspended or expired qualification means the asset
//                      lacks a compliant operator.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.CraneAndHoist.ValueObjects;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.CraneAndHoist.Services;

/// <summary>
/// Evaluates ASME B30 (crane/hoist/lifting-device) compliance for a site's registered
/// crane/hoist assets. Pure/stateless: callers project the real ICraneHoistSystemsRepository
/// bulk query results into the snapshot records this method accepts.
/// </summary>
public sealed class CraneAndHoistComplianceEngine : IDomainService
{
    private const string EngineCode = "ASME_B30";

    /// <summary>Window (days), prior to expiry, within which a not-yet-expired operator qualification is flagged with a Medium warning.</summary>
    private const int OperatorQualificationApproachingExpiryWindowDays = 30;

    /// <summary>Window (days), prior to due date, within which a not-yet-overdue inspection is flagged with a Medium warning.</summary>
    private const int InspectionApproachingDueWindowDays = 14;

    /// <summary>Evaluates ASME B30 compliance for the supplied site snapshot.</summary>
    public Result<StandardsComplianceReport> EvaluateCompliance(CraneAndHoistComplianceInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");

        foreach (var asset in input.Assets)
        {
            if (asset.RatedLoadCapacityKg < 0m)
                return Result<StandardsComplianceReport>.Failure(
                    $"Asset '{asset.AssetTag}': RatedLoadCapacityKg must not be negative.");
        }

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var asset in input.Assets)
            EvaluateAsset(asset, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateAsset(
        CraneHoistAssetSnapshot asset,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        // 1. Past-due periodic inspection.
        if (asset.InspectionDueAtUtc.HasValue && asset.InspectionDueAtUtc.Value.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME B30 periodic inspection interval",
                Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) is past its ASME B30 periodic inspection due date ({asset.InspectionDueAtUtc}).",
                Severity: "Critical",
                RelatedEntityId: asset.AssetTag));

            requiredActions.Add(
                $"Schedule and complete the overdue ASME B30 periodic inspection for asset {asset.AssetTag} before further use.");
        }
        else if (asset.InspectionDueAtUtc.HasValue)
        {
            var daysRemaining = (asset.InspectionDueAtUtc.Value.Value - evaluatedAtUtc.Value).TotalDays;
            if (daysRemaining <= InspectionApproachingDueWindowDays)
            {
                warnings.Add(new StandardsComplianceFinding(
                    Citation: "ASME B30 periodic inspection interval",
                    Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) has a periodic inspection due within {InspectionApproachingDueWindowDays} days ({asset.InspectionDueAtUtc}, approximately {Math.Max(0, daysRemaining):F1} day(s) remaining).",
                    Severity: "Medium",
                    RelatedEntityId: asset.AssetTag));
            }
        }

        // 2. Failed last inspection.
        if (asset.LastInspectionPassed == false)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME B30.2/B30.10 inspection deficiency",
                Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) failed its most recent inspection ({asset.LastInspectionAtUtc}) and the deficiency must be resolved before continued use.",
                Severity: "Critical",
                RelatedEntityId: asset.AssetTag));

            requiredActions.Add(
                $"Resolve the inspection deficiency on asset {asset.AssetTag} and re-inspect before returning it to service.");
        }

        // 3. Overload.
        var isOverloaded = asset.OverloadFlagged
            || (asset.OverloadEventCount > 0 && asset.LastObservedLoadKg.HasValue && asset.LastObservedLoadKg.Value > asset.RatedLoadCapacityKg);

        if (isOverloaded)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME B30 rated load capacity",
                Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) has one or more recorded overload events (rated capacity {asset.RatedLoadCapacityKg:N0} kg, last observed load {asset.LastObservedLoadKg:N0} kg, overload event count {asset.OverloadEventCount}).",
                Severity: "Critical",
                RelatedEntityId: asset.AssetTag));

            requiredActions.Add(
                $"Remove asset {asset.AssetTag} from service and inspect for damage following its overload event(s) before returning it to service.");
        }

        // 4. Rigging defect.
        if (asset.LastRiggingDefectDetected)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME B30.9/B30.10 rigging/hook inspection",
                Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) has a rigging or hook defect detected during its last rigging inspection ({asset.LastRiggingInspectionAtUtc}).",
                Severity: "High",
                RelatedEntityId: asset.AssetTag));

            requiredActions.Add(
                $"Replace or repair the defective rigging/hook component on asset {asset.AssetTag} before further lifting operations.");
        }

        // 5. Operator qualification missing/suspended/expired.
        var lacksQualifiedOperator = asset.OperatorQualificationSuspended
            || asset.OperatorQualifiedAtUtc is null
            || (asset.OperatorQualificationExpiresAtUtc.HasValue && asset.OperatorQualificationExpiresAtUtc.Value.Value < evaluatedAtUtc.Value);

        if (lacksQualifiedOperator)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME B30 qualified/designated operator requirement",
                Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag}) lacks a currently-qualified operator (qualified: {asset.OperatorQualifiedAtUtc}, expires: {asset.OperatorQualificationExpiresAtUtc}, suspended: {asset.OperatorQualificationSuspended}).",
                Severity: "High",
                RelatedEntityId: asset.AssetTag));

            requiredActions.Add(
                $"Assign a currently-qualified/designated operator to asset {asset.AssetTag} before further use.");
        }
        else if (asset.OperatorQualificationExpiresAtUtc.HasValue)
        {
            var daysToExpiry = (asset.OperatorQualificationExpiresAtUtc.Value.Value - evaluatedAtUtc.Value).TotalDays;
            if (daysToExpiry <= OperatorQualificationApproachingExpiryWindowDays)
            {
                warnings.Add(new StandardsComplianceFinding(
                    Citation: "ASME B30 qualified/designated operator requirement",
                    Description: $"Asset '{asset.AssetTitle}' ({asset.AssetTag})'s operator qualification expires within {OperatorQualificationApproachingExpiryWindowDays} days ({asset.OperatorQualificationExpiresAtUtc}, approximately {Math.Max(0, daysToExpiry):F1} day(s) remaining).",
                    Severity: "Medium",
                    RelatedEntityId: asset.AssetTag));
            }
        }
    }
}
