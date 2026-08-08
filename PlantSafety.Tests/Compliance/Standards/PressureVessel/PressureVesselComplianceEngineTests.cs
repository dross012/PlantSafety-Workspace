// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.PressureVessel
//  Layer:          Tests (Unit)
//  File:           PressureVesselComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the PressureVesselComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.PressureVessel.Services;

namespace PlantSafety.Tests.Compliance.Standards.PressureVessel;

/// <summary>Unit tests for <see cref="PressureVesselComplianceEngine"/>.</summary>
public sealed class PressureVesselComplianceEngineTests : UnitTestBase
{
    private readonly PressureVesselComplianceEngine _engine = new();

    [Fact]
    public void EvaluateCompliance_OverdueInspection_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "BOILER-01", VesselType: "Boiler", MaxAllowableWorkingPressurePsi: 150m,
                    LastInspectionAtUtc: new UtcTimestamp(now.Value.AddYears(-2)),
                    NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddDays(-5)),
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: false,
                    InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation.Contains("inspection requirement") && v.RelatedEntityId == "BOILER-01");
    }

    [Fact]
    public void EvaluateCompliance_MissingJurisdictionalCertificate_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "RECEIVER-02", VesselType: "Air Receiver", MaxAllowableWorkingPressurePsi: 200m,
                    LastInspectionAtUtc: new UtcTimestamp(now.Value.AddMonths(-6)),
                    NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddYears(1)),
                    HasCurrentJurisdictionalCertificate: false,
                    NewOrReplacementInstallationPending: false,
                    InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation.Contains("operating certificate"));
    }

    [Fact]
    public void EvaluateCompliance_PendingInstallationWithNoNotice_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "STORAGE-03", VesselType: "Storage Vessel", MaxAllowableWorkingPressurePsi: 100m,
                    LastInspectionAtUtc: null,
                    NextInspectionDueAtUtc: null,
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: true,
                    InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Description.Contains("30 days"));
    }

    [Fact]
    public void EvaluateCompliance_PendingInstallationNoticeFiledTooRecently_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "STORAGE-04", VesselType: "Storage Vessel", MaxAllowableWorkingPressurePsi: 100m,
                    LastInspectionAtUtc: null,
                    NextInspectionDueAtUtc: null,
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: true,
                    InstallationNoticeFiledAtUtc: new UtcTimestamp(now.Value.AddDays(-10))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "High");
    }

    [Fact]
    public void EvaluateCompliance_PendingInstallationNoticeFiledWellInAdvance_ShouldNotBeFlagged()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "STORAGE-05", VesselType: "Storage Vessel", MaxAllowableWorkingPressurePsi: 100m,
                    LastInspectionAtUtc: null,
                    NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddYears(1)),
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: true,
                    InstallationNoticeFiledAtUtc: new UtcTimestamp(now.Value.AddDays(-45))),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_InspectionApproachingWithinThirtyDays_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "BOILER-06", VesselType: "Boiler", MaxAllowableWorkingPressurePsi: 150m,
                    LastInspectionAtUtc: new UtcTimestamp(now.Value.AddYears(-1)),
                    NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddDays(10)),
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: false,
                    InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "Medium" && w.Description.Contains("2-4 weeks"));
    }

    [Fact]
    public void EvaluateCompliance_FullyCompliantVessel_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "BOILER-07", VesselType: "Boiler", MaxAllowableWorkingPressurePsi: 150m,
                    LastInspectionAtUtc: new UtcTimestamp(now.Value.AddMonths(-6)),
                    NextInspectionDueAtUtc: new UtcTimestamp(now.Value.AddYears(1)),
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: false,
                    InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_EmptyVesselIdentifier_ShouldFail()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: string.Empty, VesselType: "Boiler", MaxAllowableWorkingPressurePsi: 150m,
                    LastInspectionAtUtc: null, NextInspectionDueAtUtc: null,
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: false, InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EvaluateCompliance_NegativePressure_ShouldFail()
    {
        var now = UtcTimestamp.Now();

        var input = new PressureVesselComplianceInput(
            Vessels:
            [
                new PressureVesselRecord(
                    VesselIdentifier: "BOILER-08", VesselType: "Boiler", MaxAllowableWorkingPressurePsi: -10m,
                    LastInspectionAtUtc: null, NextInspectionDueAtUtc: null,
                    HasCurrentJurisdictionalCertificate: true,
                    NewOrReplacementInstallationPending: false, InstallationNoticeFiledAtUtc: null),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
