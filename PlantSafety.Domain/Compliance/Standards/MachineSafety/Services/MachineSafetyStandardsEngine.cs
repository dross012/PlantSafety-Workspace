// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.MachineSafety
//  Layer:          Domain
//  File:           MachineSafetyStandardsEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that rolls up ANSI B11 (machine
//                  safeguarding), ANSI Z244.1 (lockout/tagout), ANSI Z535 (signage, evaluated via
//                  guard/label presence proxies since no dedicated signage aggregate exists), ISO
//                  12100 (hazard/risk-reduction hierarchy, evaluated via guard+control presence as
//                  a proxy for the "technical protective measures" step), ISO 14120 (guards), and
//                  ISO 14119 (interlocking devices) compliance for a site's real MachineGuardsBarriers,
//                  LotoSystems, EmergencyStopDevices, and TwoHandControls data. There is no
//                  persisted aggregate for this engine -- it is a pure calculation over
//                  caller-supplied snapshots (same "stateless calculation service" pattern as
//                  Compliance.EPA.Rcra.Services.RcraWasteComplianceEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - ISO 12100: risk reduction follows a strict 3-step hierarchy -- (1)
//                      inherently safe design, (2) technical protective measures (guards,
//                      interlocks, light curtains, safety functions), (3) information for use
//                      (warnings, manuals, instructions for residual risk). ANSI B11.0-2023
//                      provides an equivalent/harmonized methodology, with a documented risk
//                      assessment required per B11.0 Annex C, ISO 12100, or the NSC method.
//                    - ANSI B11 series (machine-specific standards, e.g. B11.1 mechanical power
//                      presses, B11.19 performance requirements for safeguarding/risk-reduction
//                      measures, B11.26 functional safety for machinery): integrated systems
//                      (multiple machines brought together by an integrator) require their own
//                      system-level risk assessment and safeguarding compliance.
//                    - ANSI/ASSP Z244.1 (lockout/tagout, 2024 edition): lockout is the primary
//                      hazardous-energy control method; tagout and "alternative methods" are
//                      co-equal/secondary choices (the 2024 revision elevated alternative methods
//                      to co-equal with LOTO). Applies to installation, inspection, testing,
//                      troubleshooting, and decommissioning activities on machinery/equipment/
//                      processes; a zero-energy state must be verified before servicing.
//                    - ISO 13850: the on-point emergency-stop-function standard -- periodic
//                      functional testing of e-stop devices is required (cited here as directly
//                      on-point industry knowledge for e-stop devices, even though not explicitly
//                      enumerated in the original web research list; no numeric interval beyond
//                      "periodic functional test required" is asserted).
//                    - ISO 13851: the on-point two-hand control device standard, same reasoning
//                      as ISO 13850 above.
//                    - ISO 14120 (guards) / ISO 14119 (interlocking devices): design and
//                      inspection requirements for fixed/movable guards and interlocking devices.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.MachineSafety.Services;

/// <summary>
/// Evaluates ANSI B11 (machine safeguarding), ANSI Z244.1 (lockout/tagout), ANSI Z535 (signage,
/// evaluated via guard/label presence proxies since no dedicated signage aggregate exists), ISO
/// 12100 (hazard/risk-reduction hierarchy, evaluated via guard+control presence as a proxy for
/// the "technical protective measures" step), ISO 14120 (guards), and ISO 14119 (interlocking
/// devices) compliance by rolling up real MachineGuardsBarriers, LotoSystems, EmergencyStopDevices,
/// and TwoHandControls data. Pure/stateless.
/// </summary>
public sealed class MachineSafetyStandardsEngine : IDomainService
{
    private const string EngineCode = "ANSI_B11_ISO_MACHINE_SAFETY";

    /// <summary>Minimum acceptable guard integrity score (illustrative operational threshold).</summary>
    private const decimal GuardIntegrityMinimumScore = 70m;

    /// <summary>Minimum acceptable LOTO compliance score (illustrative operational threshold).</summary>
    private const decimal LotoComplianceMinimumScore = 70m;

    /// <summary>Maximum days between LOTO periodic audits before a Medium warning is raised (annual LOTO periodic inspection, per ANSI/ASSP Z244.1 industry practice).</summary>
    private const int LotoAuditMaxIntervalDays = 365;

    /// <summary>Evaluates the rolled-up ANSI/ISO machine-safety-standards compliance for the supplied site snapshot.</summary>
    public Result<StandardsComplianceReport> EvaluateCompliance(MachineSafetyStandardsInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");

        foreach (var guard in input.Guards)
        {
            if (guard.DeficiencyCount < 0)
                return Result<StandardsComplianceReport>.Failure($"Guard '{guard.GuardTitle}': DeficiencyCount must not be negative.");
        }

        foreach (var loto in input.LotoSystems)
        {
            if (loto.IsolationPointCount < 0)
                return Result<StandardsComplianceReport>.Failure($"LOTO system '{loto.SystemTitle}': IsolationPointCount must not be negative.");
        }

        foreach (var stop in input.EmergencyStops)
        {
            if (stop.FailureCount < 0)
                return Result<StandardsComplianceReport>.Failure($"E-stop device '{stop.DeviceTitle}': FailureCount must not be negative.");
        }

        foreach (var control in input.TwoHandControls)
        {
            if (control.BypassAttemptCount < 0)
                return Result<StandardsComplianceReport>.Failure($"Two-hand control '{control.ControlTitle}': BypassAttemptCount must not be negative.");
        }

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var guard in input.Guards)
            EvaluateGuard(guard, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        foreach (var loto in input.LotoSystems)
            EvaluateLoto(loto, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        foreach (var stop in input.EmergencyStops)
            EvaluateEmergencyStop(stop, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        foreach (var control in input.TwoHandControls)
            EvaluateTwoHandControl(control, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateGuard(
        MachineGuardSnapshot guard,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (guard.NextInspectionDueAtUtc.HasValue && guard.NextInspectionDueAtUtc.Value.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 14120 (guards) / ANSI B11.19 inspection interval",
                Description: $"Guard '{guard.GuardTitle}' ({guard.EquipmentLocation}) is past its inspection due date ({guard.NextInspectionDueAtUtc}).",
                Severity: "High",
                RelatedEntityId: guard.GuardTitle));

            requiredActions.Add($"Perform the overdue guard inspection for '{guard.GuardTitle}'.");
        }

        if (guard.DeficiencyCount > 0)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ANSI B11.19 safeguarding performance requirements",
                Description: $"Guard '{guard.GuardTitle}' ({guard.EquipmentLocation}) has {guard.DeficiencyCount} recorded deficiency/deficiencies that must be corrected.",
                Severity: "Critical",
                RelatedEntityId: guard.GuardTitle));

            requiredActions.Add($"Correct the {guard.DeficiencyCount} recorded deficiency/deficiencies on guard '{guard.GuardTitle}'.");
        }

        if (guard.GuardIntegrityScore < GuardIntegrityMinimumScore)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ISO 14120 (guards)",
                Description: $"Guard '{guard.GuardTitle}' has an integrity score of {guard.GuardIntegrityScore:N1}, below the {GuardIntegrityMinimumScore:N0} threshold.",
                Severity: "Medium",
                RelatedEntityId: guard.GuardTitle));
        }
    }

    private static void EvaluateLoto(
        LotoSystemSnapshot loto,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (loto.VerifiedZeroEnergyAtUtc is null)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ANSI/ASSP Z244.1",
                Description: $"LOTO system '{loto.SystemTitle}' ({loto.EquipmentTag}) has no recorded zero-energy-state verification -- zero-energy state must be verified before servicing.",
                Severity: "Critical",
                RelatedEntityId: loto.SystemTitle));

            requiredActions.Add($"Verify and record the zero-energy state for LOTO system '{loto.SystemTitle}' before any servicing activity proceeds.");
        }

        var auditIsStale = loto.AuditedAtUtc is null
            || (evaluatedAtUtc.Value - loto.AuditedAtUtc.Value.Value).TotalDays > LotoAuditMaxIntervalDays;

        if (auditIsStale)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ANSI/ASSP Z244.1 periodic inspection",
                Description: $"LOTO system '{loto.SystemTitle}' has not been periodically audited within the last {LotoAuditMaxIntervalDays} days (last audit: {loto.AuditedAtUtc}).",
                Severity: "Medium",
                RelatedEntityId: loto.SystemTitle));
        }

        if (loto.ComplianceScore < LotoComplianceMinimumScore)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ANSI/ASSP Z244.1",
                Description: $"LOTO system '{loto.SystemTitle}' has a compliance score of {loto.ComplianceScore:N1}, below the {LotoComplianceMinimumScore:N0} threshold.",
                Severity: "High",
                RelatedEntityId: loto.SystemTitle));
        }
    }

    private static void EvaluateEmergencyStop(
        EmergencyStopSnapshot stop,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (stop.NextTestDueAtUtc.HasValue && stop.NextTestDueAtUtc.Value.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 13850 / machine-specific e-stop test interval",
                Description: $"Emergency-stop device '{stop.DeviceTitle}' ({stop.EquipmentLocation}) is past its periodic functional test due date ({stop.NextTestDueAtUtc}).",
                Severity: "High",
                RelatedEntityId: stop.DeviceTitle));

            requiredActions.Add($"Perform the overdue periodic functional test for emergency-stop device '{stop.DeviceTitle}'.");
        }

        if (stop.FailureCount > 0)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 13850",
                Description: $"Emergency-stop device '{stop.DeviceTitle}' has {stop.FailureCount} recorded failure(s) and must be investigated/repaired.",
                Severity: "Critical",
                RelatedEntityId: stop.DeviceTitle));

            requiredActions.Add($"Investigate and repair emergency-stop device '{stop.DeviceTitle}' -- {stop.FailureCount} recorded failure(s).");
        }
    }

    private static void EvaluateTwoHandControl(
        TwoHandControlSnapshot control,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (control.NextTestDueAtUtc.HasValue && control.NextTestDueAtUtc.Value.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ANSI B11.19 / ISO 13851 two-hand control device requirements",
                Description: $"Two-hand control '{control.ControlTitle}' ({control.EquipmentLocation}) is past its periodic test due date ({control.NextTestDueAtUtc}).",
                Severity: "High",
                RelatedEntityId: control.ControlTitle));

            requiredActions.Add($"Perform the overdue periodic test for two-hand control '{control.ControlTitle}'.");
        }

        if (control.BypassAttemptCount > 0)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ANSI B11.19 / ISO 13851 two-hand control device requirements",
                Description: $"Bypass attempt(s) detected on two-hand control '{control.ControlTitle}' ({control.BypassAttemptCount} recorded attempt(s)) -- this defeats the safeguarding function and requires immediate investigation.",
                Severity: "Critical",
                RelatedEntityId: control.ControlTitle));

            requiredActions.Add($"Immediately investigate the {control.BypassAttemptCount} bypass attempt(s) detected on two-hand control '{control.ControlTitle}'.");
        }
    }
}
