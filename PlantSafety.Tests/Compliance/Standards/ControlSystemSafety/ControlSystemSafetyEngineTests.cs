// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.ControlSystemSafety
//  Layer:          Tests (Unit)
//  File:           ControlSystemSafetyEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ControlSystemSafetyEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Services;

namespace PlantSafety.Tests.Compliance.Standards.ControlSystemSafety;

/// <summary>Unit tests for <see cref="ControlSystemSafetyEngine"/>.</summary>
public sealed class ControlSystemSafetyEngineTests : UnitTestBase
{
    private readonly ControlSystemSafetyEngine _engine = new();

    private static SafetyComponentSnapshot CompliantComponent(UtcTimestamp now, string title = "Press Guard Relay") => new(
        ComponentTitle: title,
        ComponentType: "DualChannelSafetyRelay",
        EquipmentLocation: "Press Line 1",
        RequiredPerformanceLevel: PerformanceLevel.PLd,
        AchievedPerformanceLevel: PerformanceLevel.PLd,
        Category: ControlSystemCategory.Category3,
        InspectedAtUtc: new UtcTimestamp(now.Value.AddDays(-30)),
        NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddDays(60)),
        FaultCount: 0,
        ReliabilityOrAccuracyScore: 95m);

    [Fact]
    public void EvaluateCompliance_AllCompliant_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();
        var input = new ControlSystemSafetyInput([CompliantComponent(now)], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_AchievedPlBelowRequired_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with
        {
            RequiredPerformanceLevel = PerformanceLevel.PLd,
            AchievedPerformanceLevel = PerformanceLevel.PLb,
        };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation == "ISO 13849-1");
    }

    [Fact]
    public void EvaluateCompliance_NoFormalPlRating_ShouldReportHighWarning()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with { AchievedPerformanceLevel = null, Category = null };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w =>
            w.Severity == "High" && w.Description.Contains("no formally rated"));
    }

    [Fact]
    public void EvaluateCompliance_PastInspectionInterval_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with { NextInspectionDueAtUtc = new UtcTimestamp(now.Value.AddDays(-1)) };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "High" && v.Citation == "ISO 13849-1");
    }

    [Fact]
    public void EvaluateCompliance_FaultHistory_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with { FaultCount = 3 };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Warnings.Should().Contain(w => w.Severity == "Medium" && w.Description.Contains("fault history"));
    }

    [Fact]
    public void EvaluateCompliance_ReliabilityScoreBelowThreshold_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with { ReliabilityOrAccuracyScore = 65m };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Warnings.Should().Contain(w => w.Severity == "Medium" && w.Description.Contains("reliability/detection-accuracy score"));
    }

    [Fact]
    public void EvaluateCompliance_NegativeFaultCount_ShouldFail()
    {
        var now = UtcTimestamp.Now();
        var component = CompliantComponent(now) with { FaultCount = -1 };
        var input = new ControlSystemSafetyInput([component], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
