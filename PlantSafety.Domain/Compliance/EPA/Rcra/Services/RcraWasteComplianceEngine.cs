// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Rcra
//  Layer:          Domain
//  File:           RcraWasteComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates RCRA
//                  (40 CFR 260-279) hazardous-waste-management compliance for a site's current
//                  waste containers, accumulation areas, and manifests. There is no persisted
//                  aggregate for this engine -- it is a pure calculation over caller-supplied
//                  snapshots of the real WasteContainer / HazardousWasteManifest aggregate state
//                  (same "stateless calculation service" pattern as
//                  Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - Generator status thresholds: 40 CFR 262 Subpart C (262.13, 262.14, 262.16).
//                      LQG >= 1,000 kg hazardous waste/month, or > 1 kg acute hazardous waste/month;
//                      SQG 100-999 kg/month; VSQG &lt;= 100 kg/month (and &lt;= 1 kg acute/month).
//                    - LQG on-site accumulation time limit: 90 days (40 CFR 262.17(b)); a Regional
//                      Administrator may grant a 30-day extension for unforeseen circumstances.
//                    - SQG on-site accumulation time limit: 180 days, or 270 days if the SQG must
//                      ship hazardous waste > 200 miles for treatment/storage/disposal; SQG on-site
//                      quantity must never exceed 6,000 kg. Any site accumulating > 1 kg of acute
//                      hazardous waste must follow LQG standards for that waste regardless of
//                      overall generator status.
//                    - Manifest discrepancy handling: 40 CFR 262 Subpart D.
//                    - LQG exception-report trigger (illustrative simplification, see step 6 below):
//                      40 CFR 262.42.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Rcra.Enums;
using PlantSafety.Domain.Compliance.EPA.Rcra.ValueObjects;
using PlantSafety.Domain.Compliance.EPA.Shared;
using PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.EPA.Rcra.Services;

/// <summary>
/// Evaluates RCRA (40 CFR 260-279) hazardous-waste-management compliance for a site's current
/// waste containers, accumulation areas, and manifests. Pure/stateless: callers must load the
/// real WasteContainer/AccumulationArea/HazardousWasteManifest aggregates via their repository
/// interfaces (PlantSafety.Domain.HazardousWasteManagement.*.Interfaces.I*Repository) and project
/// them into the snapshot records this method accepts.
/// </summary>
public sealed class RcraWasteComplianceEngine : IDomainService
{
    private const string ProgramCode = "RCRA";

    /// <summary>Container time-remaining window (days) at which an open container is flagged with a Medium warning.</summary>
    private const int ApproachingDeadlineWindowDays = 14;

    /// <summary>SQG maximum on-site accumulation quantity per 40 CFR 262.16 (kg).</summary>
    private const decimal SqgMaxOnSiteAccumulationKg = 6_000m;

    /// <summary>
    /// Illustrative operational threshold (not a literal regulatory deadline): the number of days
    /// after manifest generation, still un-signed by a transporter, after which this engine
    /// recommends the generator investigate per the LQG exception-reporting concept in 40 CFR 262.42.
    /// </summary>
    private const int UnsignedManifestInvestigationWindowDays = 35;

    /// <summary>Evaluates RCRA compliance for the supplied site snapshot.</summary>
    public Result<EpaComplianceReport> EvaluateCompliance(RcraComplianceInput input)
    {
        if (input is null)
            return Result<EpaComplianceReport>.Failure("Input is required.");
        if (input.MonthlyHazardousWasteQuantityKg < 0m)
            return Result<EpaComplianceReport>.Failure("MonthlyHazardousWasteQuantityKg must not be negative.");
        if (input.MonthlyAcuteHazardousWasteQuantityKg < 0m)
            return Result<EpaComplianceReport>.Failure("MonthlyAcuteHazardousWasteQuantityKg must not be negative.");

        var violations = new List<EpaComplianceFinding>();
        var warnings = new List<EpaComplianceFinding>();
        var requiredActions = new List<string>();

        var generatorStatus = ClassifyGeneratorStatus(
            input.MonthlyHazardousWasteQuantityKg, input.MonthlyAcuteHazardousWasteQuantityKg);

        EvaluateContainers(input, violations, warnings, requiredActions);
        EvaluateSqgAccumulationLimit(input, generatorStatus, warnings);
        EvaluateManifests(input, violations, warnings, requiredActions);

        var report = new EpaComplianceReport(
            ProgramCode: ProgramCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<EpaComplianceReport>.Success(report);
    }

    /// <summary>
    /// Classifies RCRA generator status from monthly-generated hazardous-waste quantities per
    /// 40 CFR 262 Subpart C. Pure, deterministic: LQG if total &gt;= 1,000 kg OR acute &gt; 1 kg;
    /// SQG if 100-999 kg total (and acute &lt;= 1 kg); otherwise VSQG.
    /// </summary>
    public static RcraGeneratorStatus ClassifyGeneratorStatus(decimal monthlyTotalKg, decimal monthlyAcuteKg)
    {
        if (monthlyTotalKg >= 1_000m || monthlyAcuteKg > 1m)
            return RcraGeneratorStatus.LargeQuantityGenerator;

        if (monthlyTotalKg >= 100m)
            return RcraGeneratorStatus.SmallQuantityGenerator;

        return RcraGeneratorStatus.VerySmallQuantityGenerator;
    }

    private static void EvaluateContainers(
        RcraComplianceInput input,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        foreach (var container in input.Containers)
        {
            var isOpen = !string.Equals(container.Status, "Closed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(container.Status, "Shipped", StringComparison.OrdinalIgnoreCase);

            if (!isOpen)
                continue;

            // Mirrors WasteContainer.TimeLimitDeadlineUtc()/IsPastTimeLimit() logic exactly.
            var deadlineUtc = container.OpenedAtUtc.Value.AddDays(container.RegulatoryTimeLimitDays);
            var nowUtc = input.EvaluatedAtUtc.Value;
            var isPastTimeLimit = nowUtc > deadlineUtc;

            if (isPastTimeLimit)
            {
                var citation = container.RegulatoryTimeLimitDays <= 90
                    ? "40 CFR 262.17(b)"
                    : "40 CFR 262.16 (SQG 180/270-day accumulation limit)";

                violations.Add(new EpaComplianceFinding(
                    Citation: citation,
                    Description: $"Container '{container.ContainerLabel}' has exceeded its {container.RegulatoryTimeLimitDays}-day RCRA accumulation time limit (opened {container.OpenedAtUtc}, deadline {deadlineUtc:O}).",
                    Severity: "Critical",
                    RelatedEntityId: container.WasteContainerId.ToString()));

                requiredActions.Add(
                    $"Ship or properly dispose of container {container.ContainerLabel} immediately -- it has exceeded its {container.RegulatoryTimeLimitDays}-day RCRA accumulation limit.");
            }
            else
            {
                var daysRemaining = (deadlineUtc - nowUtc).TotalDays;
                if (daysRemaining <= ApproachingDeadlineWindowDays)
                {
                    warnings.Add(new EpaComplianceFinding(
                        Citation: "40 CFR 262.17(b) / 262.16",
                        Description: $"Container '{container.ContainerLabel}' is approaching its {container.RegulatoryTimeLimitDays}-day RCRA accumulation time limit (deadline {deadlineUtc:O}, approximately {Math.Max(0, daysRemaining):F1} day(s) remaining).",
                        Severity: "Medium",
                        RelatedEntityId: container.WasteContainerId.ToString()));

                    requiredActions.Add(
                        $"Plan to close and ship container {container.ContainerLabel} before its {container.RegulatoryTimeLimitDays}-day RCRA accumulation deadline of {deadlineUtc:O}.");
                }
            }
        }
    }

    private static void EvaluateSqgAccumulationLimit(
        RcraComplianceInput input,
        RcraGeneratorStatus generatorStatus,
        List<EpaComplianceFinding> warnings)
    {
        if (generatorStatus != RcraGeneratorStatus.SmallQuantityGenerator)
            return;

        if (input.MonthlyHazardousWasteQuantityKg <= SqgMaxOnSiteAccumulationKg)
            return;

        // This engine only has monthly-generation figures, not per-container weights, so it cannot
        // determine actual on-site accumulated quantity across containers -- flag for manual
        // verification rather than asserting a violation it cannot substantiate.
        warnings.Add(new EpaComplianceFinding(
            Citation: "40 CFR 262.16",
            Description: $"Monthly generated quantity ({input.MonthlyHazardousWasteQuantityKg:N0} kg) exceeds the SQG 6,000 kg on-site accumulation limit. This engine only has monthly-generation figures, not per-container weights, so actual on-site accumulated quantity must be verified against real container contents.",
            Severity: "High",
            RelatedEntityId: null));
    }

    private static void EvaluateManifests(
        RcraComplianceInput input,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        foreach (var manifest in input.Manifests)
        {
            if (manifest.DiscrepancyReason is not null
                && !string.Equals(manifest.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new EpaComplianceFinding(
                    Citation: "40 CFR 262 Subpart D",
                    Description: $"Manifest '{manifest.ManifestNumber}' has an unresolved discrepancy: {manifest.DiscrepancyReason}",
                    Severity: "High",
                    RelatedEntityId: manifest.ManifestId.ToString()));

                requiredActions.Add(
                    $"Resolve the discrepancy on manifest {manifest.ManifestNumber} before further shipment activity: {manifest.DiscrepancyReason}");
            }

            var isStillGenerated = string.Equals(manifest.Status, "Generated", StringComparison.OrdinalIgnoreCase)
                && manifest.TransporterSignedAtUtc is null;

            if (isStillGenerated)
            {
                var daysSinceGenerated = (input.EvaluatedAtUtc.Value - manifest.GeneratedAtUtc.Value).TotalDays;
                if (daysSinceGenerated > UnsignedManifestInvestigationWindowDays)
                {
                    warnings.Add(new EpaComplianceFinding(
                        Citation: "40 CFR 262.42 (illustrative simplification of the LQG exception-report trigger)",
                        Description: $"Manifest '{manifest.ManifestNumber}' has not received a transporter signature {daysSinceGenerated:F0} days after generation ({manifest.GeneratedAtUtc}). Investigate whether the shipment was received.",
                        Severity: "Medium",
                        RelatedEntityId: manifest.ManifestId.ToString()));

                    requiredActions.Add(
                        $"Investigate manifest {manifest.ManifestNumber} -- no signed copy has been received back {daysSinceGenerated:F0} days after generation.");
                }
            }
        }
    }
}
