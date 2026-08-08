// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.OperationalSafety
//  Layer:          Tests (Unit)
//  File:           OperationalSafetyStandardsEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the OperationalSafetyStandardsEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.OperationalSafety.Services;

namespace PlantSafety.Tests.Compliance.Standards.OperationalSafety;

/// <summary>Unit tests for <see cref="OperationalSafetyStandardsEngine"/>.</summary>
public sealed class OperationalSafetyStandardsEngineTests : UnitTestBase
{
    private readonly OperationalSafetyStandardsEngine _engine = new();

    [Fact]
    public void EvaluateCompliance_NullSafetyManagementSystem_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [],
            ConfinedSpacePermits: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Warnings.Should().ContainSingle(w => w.Severity == "Medium" && w.Description.Contains("No documented safety management system"));
    }

    [Fact]
    public void EvaluateCompliance_MissingManagementSystemElement_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: new SafetyManagementSystemSnapshot(
                HasDocumentedPolicy: true,
                HasPlanningProcess: true,
                HasImplementationSupport: true,
                HasPerformanceEvaluation: false,
                HasContinualImprovementProcess: true),
            SurfaceHazards: [],
            ConfinedSpacePermits: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation.Contains("ANSI Z10 / ISO 45001") && v.Description.Contains("Performance Evaluation"));
    }

    [Fact]
    public void EvaluateCompliance_UncorrectedSurfaceHazard_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [new WalkingWorkingSurfaceHazardRecord("Warehouse Bay 3", "Uncovered floor opening", Corrected: false)],
            ConfinedSpacePermits: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "High" && v.Citation.Contains("ANSI A1264"));
    }

    [Fact]
    public void EvaluateCompliance_CorrectedSurfaceHazard_ShouldNotBeFlagged()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [new WalkingWorkingSurfaceHazardRecord("Warehouse Bay 3", "Wet floor", Corrected: true)],
            ConfinedSpacePermits: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_ActivePermitWithNoAttendant_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [],
            ConfinedSpacePermits:
            [
                new ConfinedSpacePermitSnapshot(
                    PermitId: Guid.NewGuid(), SpaceIdentifier: "TANK-04", AttendantName: null, Status: "Active",
                    IssuedAtUtc: now, ExpiresAtUtc: new UtcTimestamp(now.Value.AddDays(1))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation.Contains("ANSI Z117") && v.Description.Contains("no designated attendant"));
    }

    [Fact]
    public void EvaluateCompliance_ExpiredActivePermit_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [],
            ConfinedSpacePermits:
            [
                new ConfinedSpacePermitSnapshot(
                    PermitId: Guid.NewGuid(), SpaceIdentifier: "TANK-05", AttendantName: "J. Rivera", Status: "Active",
                    IssuedAtUtc: new UtcTimestamp(now.Value.AddDays(-10)), ExpiresAtUtc: new UtcTimestamp(now.Value.AddDays(-1))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "High" && v.Description.Contains("expired"));
    }

    [Fact]
    public void EvaluateCompliance_ClosedPermitWithNoAttendant_ShouldNotBeFlagged()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: null,
            SurfaceHazards: [],
            ConfinedSpacePermits:
            [
                new ConfinedSpacePermitSnapshot(
                    PermitId: Guid.NewGuid(), SpaceIdentifier: "TANK-06", AttendantName: null, Status: "Closed",
                    IssuedAtUtc: new UtcTimestamp(now.Value.AddDays(-30)), ExpiresAtUtc: new UtcTimestamp(now.Value.AddDays(-20))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_FullyCompliantSnapshot_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();

        var input = new OperationalSafetyStandardsInput(
            SafetyManagementSystem: new SafetyManagementSystemSnapshot(
                HasDocumentedPolicy: true,
                HasPlanningProcess: true,
                HasImplementationSupport: true,
                HasPerformanceEvaluation: true,
                HasContinualImprovementProcess: true),
            SurfaceHazards: [new WalkingWorkingSurfaceHazardRecord("Warehouse Bay 3", "Wet floor (corrected)", Corrected: true)],
            ConfinedSpacePermits:
            [
                new ConfinedSpacePermitSnapshot(
                    PermitId: Guid.NewGuid(), SpaceIdentifier: "TANK-07", AttendantName: "M. Chen", Status: "Active",
                    IssuedAtUtc: now, ExpiresAtUtc: new UtcTimestamp(now.Value.AddDays(1))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_NullInput_ShouldFail()
    {
        var result = _engine.EvaluateCompliance(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
