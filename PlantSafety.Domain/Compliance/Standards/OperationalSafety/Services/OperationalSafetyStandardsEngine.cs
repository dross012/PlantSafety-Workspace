// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyStandardsEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates ANSI Z10 / ISO 45001
//                  (occupational safety management system structural elements), ANSI A1264
//                  (walking/working surfaces), and ANSI Z117 (permit-required confined spaces)
//                  compliance. There is no persisted safety-management-system aggregate and no
//                  walking/working-surface hazard log aggregate anywhere in this codebase, so those
//                  two areas' inputs are fully caller-supplied (documented honestly, matching the
//                  EPA module's TSCA/CAA precedent for genuinely-absent domain data). Confined-space
//                  evaluation DOES consume real
//                  PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models.ConfinedSpaceItemDto
//                  data, projected into this engine's ConfinedSpacePermitSnapshot type by the calling
//                  Application-layer handler (built by a separate later effort).
//
//                  Regulatory/standards facts encoded here (verified via research, not fabricated):
//                    - ISO 45001: management-system standard for occupational health &amp; safety --
//                      establishes a comprehensive H&amp;S framework requirement (policy, planning,
//                      support, operation, performance evaluation, improvement -- standard PDCA
//                      management-system shape), not a single numeric threshold; evaluated
//                      structurally (does the org have the required management-system elements in
//                      place) rather than against a numeric limit.
//                    - ANSI Z10 (occupational safety management systems): predecessor/sibling to ISO
//                      45001 in the US context -- same general PDCA management-system shape
//                      (management leadership, planning, implementation, evaluation, review).
//                    - ANSI A1264 (walking/working surfaces): governs slip/trip/fall hazard control
//                      on walking-working surfaces -- floor/surface condition, guarding of floor
//                      openings/holes, stairway/ladder safety requirements. No specific numeric
//                      coefficient-of-friction figure was found in research, so findings here are kept
//                      qualitative (hazard present/absent, condition described) rather than inventing
//                      a numeric pass/fail threshold not sourced from research.
//                    - ANSI Z117 (confined spaces): permit-required confined space entry program
//                      requirements -- atmospheric testing, entry permit issuance, attendant
//                      presence, rescue plan. Specific PPM/threshold figures were not found in
//                      research, so findings here are kept structural (program elements
//                      present/absent), citing the standard number only.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.Standards.OperationalSafety.Services;

/// <summary>
/// Evaluates ANSI Z10 / ISO 45001 (safety management system structural elements), ANSI A1264
/// (walking/working surfaces), and ANSI Z117 (confined spaces) compliance. Pure/stateless. This
/// codebase has no real safety-management-system or walking/working-surface aggregate, so those
/// two areas' inputs are fully caller-supplied. Confined-space evaluation DOES consume real
/// Compliance.Permits.ConfinedSpace data (projected into this engine's snapshot type by the
/// calling Application-layer handler).
/// </summary>
public sealed class OperationalSafetyStandardsEngine : IDomainService
{
    private const string EngineCode = "ANSI-Z10-ISO45001-A1264-Z117";

    /// <summary>Statuses considered "no longer active" for confined-space permit evaluation purposes.</summary>
    private static readonly string[] InactivePermitStatuses = ["Closed", "Expired", "Cancelled", "Revoked", "Completed"];

    /// <summary>Evaluates ANSI Z10 / ISO 45001, ANSI A1264, and ANSI Z117 compliance for the supplied snapshot.</summary>
    public Result<StandardsComplianceReport> EvaluateCompliance(OperationalSafetyStandardsInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        EvaluateSafetyManagementSystem(input, violations, warnings, requiredActions);
        EvaluateWalkingWorkingSurfaces(input, violations, requiredActions);
        EvaluateConfinedSpacePermits(input, violations, requiredActions);

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateSafetyManagementSystem(
        OperationalSafetyStandardsInput input,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        var sms = input.SafetyManagementSystem;
        if (sms is null)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ANSI Z10 / ISO 45001",
                Description: "No documented safety management system data available for evaluation against ANSI Z10 / ISO 45001.",
                Severity: "Medium",
                RelatedEntityId: null));
            return;
        }

        AddMissingElementViolation(sms.HasDocumentedPolicy, "Policy", violations, requiredActions);
        AddMissingElementViolation(sms.HasPlanningProcess, "Planning", violations, requiredActions);
        AddMissingElementViolation(sms.HasImplementationSupport, "Support/Implementation", violations, requiredActions);
        AddMissingElementViolation(sms.HasPerformanceEvaluation, "Performance Evaluation", violations, requiredActions);
        AddMissingElementViolation(sms.HasContinualImprovementProcess, "Continual Improvement", violations, requiredActions);
    }

    private static void AddMissingElementViolation(
        bool elementPresent, string elementName, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        if (elementPresent)
            return;

        violations.Add(new StandardsComplianceFinding(
            Citation: "ANSI Z10 / ISO 45001 management-system element",
            Description: $"Missing safety management system element: {elementName}.",
            Severity: "High",
            RelatedEntityId: null));

        requiredActions.Add($"Establish the '{elementName}' element of the safety management system per ANSI Z10 / ISO 45001.");
    }

    private static void EvaluateWalkingWorkingSurfaces(
        OperationalSafetyStandardsInput input, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        foreach (var hazard in input.SurfaceHazards)
        {
            if (hazard.Corrected)
                continue;

            violations.Add(new StandardsComplianceFinding(
                Citation: "ANSI A1264 (walking/working surfaces)",
                Description: $"Uncorrected walking/working-surface hazard at '{hazard.Location}': {hazard.HazardDescription}.",
                Severity: "High",
                RelatedEntityId: hazard.Location));

            requiredActions.Add($"Correct the walking/working-surface hazard at '{hazard.Location}': {hazard.HazardDescription}.");
        }
    }

    private static void EvaluateConfinedSpacePermits(
        OperationalSafetyStandardsInput input, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        foreach (var permit in input.ConfinedSpacePermits)
        {
            var isActiveLooking = IsActiveLookingStatus(permit.Status);

            if (isActiveLooking && string.IsNullOrWhiteSpace(permit.AttendantName))
            {
                violations.Add(new StandardsComplianceFinding(
                    Citation: "ANSI Z117 confined-space attendant requirement",
                    Description: $"Confined space permit '{permit.SpaceIdentifier}' (status: {permit.Status}) has no designated attendant.",
                    Severity: "Critical",
                    RelatedEntityId: permit.PermitId.ToString()));

                requiredActions.Add($"Assign a designated attendant to confined space permit '{permit.SpaceIdentifier}' immediately per ANSI Z117.");
            }

            if (isActiveLooking && permit.ExpiresAtUtc.HasValue && permit.ExpiresAtUtc.Value.Value < input.EvaluatedAtUtc.Value)
            {
                violations.Add(new StandardsComplianceFinding(
                    Citation: "ANSI Z117 confined-space permit validity requirement",
                    Description: $"Confined space permit has expired (space: '{permit.SpaceIdentifier}', expired {permit.ExpiresAtUtc}).",
                    Severity: "High",
                    RelatedEntityId: permit.PermitId.ToString()));

                requiredActions.Add($"Close out or renew the expired confined space permit for '{permit.SpaceIdentifier}'.");
            }
        }
    }

    private static bool IsActiveLookingStatus(string status)
    {
        foreach (var inactive in InactivePermitStatuses)
        {
            if (string.Equals(status, inactive, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
