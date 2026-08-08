// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.EPA.Rcra
//  Layer:          Tests (Unit)
//  File:           RcraWasteComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the RcraWasteComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Rcra.Enums;
using PlantSafety.Domain.Compliance.EPA.Rcra.Services;
using PlantSafety.Domain.Compliance.EPA.Rcra.ValueObjects;

namespace PlantSafety.Tests.Compliance.EPA.Rcra;

/// <summary>Unit tests for <see cref="RcraWasteComplianceEngine"/>.</summary>
public sealed class RcraWasteComplianceEngineTests : UnitTestBase
{
    private readonly RcraWasteComplianceEngine _engine = new();

    [Fact]
    public void ClassifyGeneratorStatus_AtOrAbove1000Kg_ShouldBeLargeQuantityGenerator()
    {
        var status = RcraWasteComplianceEngine.ClassifyGeneratorStatus(1_000m, 0m);

        status.Should().Be(RcraGeneratorStatus.LargeQuantityGenerator);
    }

    [Fact]
    public void ClassifyGeneratorStatus_MoreThanOneKgAcute_ShouldBeLargeQuantityGeneratorRegardlessOfTotal()
    {
        var status = RcraWasteComplianceEngine.ClassifyGeneratorStatus(50m, 1.5m);

        status.Should().Be(RcraGeneratorStatus.LargeQuantityGenerator);
    }

    [Fact]
    public void ClassifyGeneratorStatus_Between100And999Kg_ShouldBeSmallQuantityGenerator()
    {
        var status = RcraWasteComplianceEngine.ClassifyGeneratorStatus(500m, 0m);

        status.Should().Be(RcraGeneratorStatus.SmallQuantityGenerator);
    }

    [Fact]
    public void ClassifyGeneratorStatus_Under100Kg_ShouldBeVerySmallQuantityGenerator()
    {
        var status = RcraWasteComplianceEngine.ClassifyGeneratorStatus(50m, 0m);

        status.Should().Be(RcraGeneratorStatus.VerySmallQuantityGenerator);
    }

    [Fact]
    public void EvaluateCompliance_LqgContainerPast90DayLimit_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var openedAt = new UtcTimestamp(now.Value.AddDays(-100));

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 1_200m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers:
            [
                new RcraContainerSnapshot(
                    Guid.NewGuid(), "DRUM-001", RegulatoryTimeLimitDays: 90,
                    OpenedAtUtc: openedAt, ClosedAtUtc: null, Status: "Open"),
            ],
            Manifests: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "40 CFR 262.17(b)" && v.RelatedEntityId != null);
        result.Value.RequiredActions.Should().Contain(a => a.Contains("DRUM-001"));
    }

    [Fact]
    public void EvaluateCompliance_ContainerApproachingDeadline_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var openedAt = new UtcTimestamp(now.Value.AddDays(-85)); // 5 days remaining of a 90-day limit

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 1_200m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers:
            [
                new RcraContainerSnapshot(
                    Guid.NewGuid(), "DRUM-002", RegulatoryTimeLimitDays: 90,
                    OpenedAtUtc: openedAt, ClosedAtUtc: null, Status: "Open"),
            ],
            Manifests: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "Medium" && w.RelatedEntityId != null);
    }

    [Fact]
    public void EvaluateCompliance_ClosedContainerPastLimit_ShouldNotBeFlagged()
    {
        var now = UtcTimestamp.Now();
        var openedAt = new UtcTimestamp(now.Value.AddDays(-200));

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 500m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers:
            [
                new RcraContainerSnapshot(
                    Guid.NewGuid(), "DRUM-003", RegulatoryTimeLimitDays: 90,
                    OpenedAtUtc: openedAt, ClosedAtUtc: new UtcTimestamp(now.Value.AddDays(-150)), Status: "Closed"),
            ],
            Manifests: [],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_ManifestWithUnresolvedDiscrepancy_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 300m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers: [],
            Manifests:
            [
                new RcraManifestSnapshot(
                    ManifestId: Guid.NewGuid(),
                    ManifestNumber: "MAN-100",
                    GeneratedAtUtc: new UtcTimestamp(now.Value.AddDays(-10)),
                    TransporterSignedAtUtc: new UtcTimestamp(now.Value.AddDays(-9)),
                    TsdfReceivedAtUtc: null,
                    ClosedAtUtc: null,
                    IsImmutable: true,
                    DiscrepancyReason: "Quantity mismatch on receipt",
                    RetentionExpiresAtUtc: new UtcTimestamp(now.Value.AddYears(3)),
                    Status: "DiscrepancyFlagged"),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "40 CFR 262 Subpart D");
    }

    [Fact]
    public void EvaluateCompliance_ManifestUnsignedBeyond35Days_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 300m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers: [],
            Manifests:
            [
                new RcraManifestSnapshot(
                    ManifestId: Guid.NewGuid(),
                    ManifestNumber: "MAN-200",
                    GeneratedAtUtc: new UtcTimestamp(now.Value.AddDays(-40)),
                    TransporterSignedAtUtc: null,
                    TsdfReceivedAtUtc: null,
                    ClosedAtUtc: null,
                    IsImmutable: false,
                    DiscrepancyReason: null,
                    RetentionExpiresAtUtc: new UtcTimestamp(now.Value.AddYears(3)),
                    Status: "Generated"),
            ],
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Warnings.Should().ContainSingle(w => w.Severity == "Medium");
    }

    [Fact]
    public void EvaluateCompliance_SqgWithinAccumulationLimit_ShouldNotReportOnSiteAccumulationWarning()
    {
        // 500 kg/month is squarely within the 100-999 kg SQG bracket and well under the 6,000 kg
        // SQG on-site accumulation cap, so no accumulation-limit warning should fire. (Note: because
        // SQG status by definition caps monthly generation at 999 kg, the >6,000 kg accumulation
        // warning branch is defensive/unreachable under this engine's own classification rules --
        // this test documents that the SQG happy path stays clean.)
        var now = UtcTimestamp.Now();

        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: 500m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers: [],
            Manifests: [],
            EvaluatedAtUtc: now);

        var status = RcraWasteComplianceEngine.ClassifyGeneratorStatus(input.MonthlyHazardousWasteQuantityKg, 0m);
        status.Should().Be(RcraGeneratorStatus.SmallQuantityGenerator);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Warnings.Should().BeEmpty();
        result.Value.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public void EvaluateCompliance_NegativeQuantity_ShouldFail()
    {
        var input = new RcraComplianceInput(
            MonthlyHazardousWasteQuantityKg: -1m,
            MonthlyAcuteHazardousWasteQuantityKg: 0m,
            Containers: [],
            Manifests: [],
            EvaluatedAtUtc: UtcTimestamp.Now());

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
