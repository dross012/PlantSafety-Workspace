// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.MachineSafety
//  Layer:          Tests (Unit)
//  File:           MachineSafetyStandardsEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the MachineSafetyStandardsEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.MachineSafety.Services;

namespace PlantSafety.Tests.Compliance.Standards.MachineSafety;

/// <summary>Unit tests for <see cref="MachineSafetyStandardsEngine"/>.</summary>
public sealed class MachineSafetyStandardsEngineTests : UnitTestBase
{
    private readonly MachineSafetyStandardsEngine _engine = new();

    private static MachineGuardSnapshot CompliantGuard(UtcTimestamp now) => new(
        GuardTitle: "Press Brake Fixed Guard",
        GuardType: "FixedGuard",
        EquipmentLocation: "Press Line 1",
        InspectedAtUtc: new UtcTimestamp(now.Value.AddDays(-20)),
        NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddDays(70)),
        DeficiencyCount: 0,
        GuardIntegrityScore: 95m,
        Status: "InService");

    private static LotoSystemSnapshot CompliantLoto(UtcTimestamp now) => new(
        SystemTitle: "Extruder LOTO",
        EquipmentTag: "EXT-01",
        EnergySourceTypes: "Electrical,Hydraulic",
        IsolationPointCount: 3,
        VerifiedZeroEnergyAtUtc: new UtcTimestamp(now.Value.AddDays(-5)),
        AuditedAtUtc: new UtcTimestamp(now.Value.AddDays(-30)),
        ComplianceScore: 95m,
        Status: "Active");

    private static EmergencyStopSnapshot CompliantEStop(UtcTimestamp now) => new(
        DeviceTitle: "Line 1 E-Stop",
        DeviceType: "PushButtonStop",
        EquipmentLocation: "Press Line 1",
        TestedAtUtc: new UtcTimestamp(now.Value.AddDays(-10)),
        NextTestDueAtUtc: new UtcTimestamp(now.Value.AddDays(20)),
        FailureCount: 0,
        Status: "Operational");

    private static TwoHandControlSnapshot CompliantTwoHand(UtcTimestamp now) => new(
        ControlTitle: "Press 3 Two-Hand Control",
        EquipmentType: "Press",
        EquipmentLocation: "Press Line 1",
        TestedAtUtc: new UtcTimestamp(now.Value.AddDays(-15)),
        NextTestDueAtUtc: new UtcTimestamp(now.Value.AddDays(45)),
        BypassAttemptCount: 0,
        Status: "Operational");

    private static MachineSafetyStandardsInput EmptyInputAt(UtcTimestamp now) =>
        new(Guards: [], LotoSystems: [], EmergencyStops: [], TwoHandControls: [], EvaluatedAtUtc: now);

    [Fact]
    public void EvaluateCompliance_AllCompliant_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();
        var input = EmptyInputAt(now) with
        {
            Guards = [CompliantGuard(now)],
            LotoSystems = [CompliantLoto(now)],
            EmergencyStops = [CompliantEStop(now)],
            TwoHandControls = [CompliantTwoHand(now)],
        };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_GuardWithDeficiencies_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var guard = CompliantGuard(now) with { DeficiencyCount = 2 };
        var input = EmptyInputAt(now) with { Guards = [guard] };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ANSI B11.19 safeguarding performance requirements");
    }

    [Fact]
    public void EvaluateCompliance_GuardPastInspectionDue_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var guard = CompliantGuard(now) with { NextInspectionDueAtUtc = new UtcTimestamp(now.Value.AddDays(-1)) };
        var input = EmptyInputAt(now) with { Guards = [guard] };

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "ISO 14120 (guards) / ANSI B11.19 inspection interval");
    }

    [Fact]
    public void EvaluateCompliance_LotoMissingZeroEnergyVerification_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var loto = CompliantLoto(now) with { VerifiedZeroEnergyAtUtc = null };
        var input = EmptyInputAt(now) with { LotoSystems = [loto] };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ANSI/ASSP Z244.1");
    }

    [Fact]
    public void EvaluateCompliance_LotoStaleAudit_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var loto = CompliantLoto(now) with { AuditedAtUtc = new UtcTimestamp(now.Value.AddDays(-400)) };
        var input = EmptyInputAt(now) with { LotoSystems = [loto] };

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Warnings.Should().Contain(w =>
            w.Severity == "Medium" && w.Citation == "ANSI/ASSP Z244.1 periodic inspection");
    }

    [Fact]
    public void EvaluateCompliance_EStopWithFailureHistory_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var stop = CompliantEStop(now) with { FailureCount = 1 };
        var input = EmptyInputAt(now) with { EmergencyStops = [stop] };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation == "ISO 13850");
    }

    [Fact]
    public void EvaluateCompliance_EStopPastTestDue_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var stop = CompliantEStop(now) with { NextTestDueAtUtc = new UtcTimestamp(now.Value.AddDays(-1)) };
        var input = EmptyInputAt(now) with { EmergencyStops = [stop] };

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "ISO 13850 / machine-specific e-stop test interval");
    }

    [Fact]
    public void EvaluateCompliance_TwoHandControlBypassAttempt_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var control = CompliantTwoHand(now) with { BypassAttemptCount = 1 };
        var input = EmptyInputAt(now) with { TwoHandControls = [control] };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ANSI B11.19 / ISO 13851 two-hand control device requirements"
            && v.Description.Contains("Bypass attempt"));
    }

    [Fact]
    public void EvaluateCompliance_NegativeDeficiencyCount_ShouldFail()
    {
        var now = UtcTimestamp.Now();
        var guard = CompliantGuard(now) with { DeficiencyCount = -1 };
        var input = EmptyInputAt(now) with { Guards = [guard] };

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
