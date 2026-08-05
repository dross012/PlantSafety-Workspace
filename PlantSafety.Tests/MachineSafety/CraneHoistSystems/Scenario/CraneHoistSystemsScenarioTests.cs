// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Scenario
//  Layer:          Tests (Scenario, in-process)
//  File:           CraneHoistSystemsScenarioTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Business-scenario tests: multi-step realistic sequences of CraneHoistSystems
//                  domain behaviors representing OSHA 1910.179/1926.1400-series operational
//                  workflows (overload -> corrective inspection cycle; qualification suspension
//                  after a rigging defect). Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Scenario;

public sealed class CraneHoistSystemsScenarioTests : UnitTestBase
{
    private static Agg NewAsset() => Agg.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        TestTenantId.Value, null, "CRANE-SCN", "Scenario Crane", CraneHoistAssetType.OverheadBridgeCrane, 5000m);

    [Fact]
    public void Scenario_OverloadTriggersUnscheduledInspectionCycle_EndsInService()
    {
        var asset = NewAsset();

        // Operator lifts a load within limits.
        asset.RecordLiftEvent(3000m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.OverloadFlagged.Should().BeFalse();

        // A subsequent lift exceeds rated capacity -- safety office schedules an unscheduled inspection.
        asset.RecordOverloadEvent(6200m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.OverloadFlagged.Should().BeTrue();
        asset.OverloadEventCount.Should().Be(1);

        asset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(1)), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.Status.Should().Be(CraneHoistSystemsStatus.InspectionDue);

        // Inspection passes; asset returns to service.
        asset.CompleteCraneInspection(true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.Status.Should().Be(CraneHoistSystemsStatus.InService);
        asset.LastInspectionPassed.Should().BeTrue();
    }

    [Fact]
    public void Scenario_RiggingDefectLeadsToOperatorQualificationSuspension()
    {
        var asset = NewAsset();
        asset.QualifyOperator(TestUserId, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestActor, TestCorrelationId, TestCausationId);

        // Routine rigging inspection detects a defect.
        asset.RecordRiggingInspection(true, "shackle pin worn beyond tolerance", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.LastRiggingDefectDetected.Should().BeTrue();

        // Safety office suspends the operator pending re-certification.
        asset.SuspendOperatorQualification("rigging defect under operator's watch", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.OperatorQualificationSuspended.Should().BeTrue();

        // Operator is re-qualified after remediation.
        asset.QualifyOperator(TestUserId, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestActor, TestCorrelationId, TestCausationId);
        asset.OperatorQualificationSuspended.Should().BeFalse();
    }

    [Fact]
    public void Scenario_FailedInspectionCycle_ResultsInOverdueStatusUntilRemediated()
    {
        var asset = NewAsset();

        asset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.CompleteCraneInspection(false, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.Status.Should().Be(CraneHoistSystemsStatus.InspectionOverdue);

        // Remediated, re-scheduled, and passes on retry.
        asset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(3)), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.CompleteCraneInspection(true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(CraneHoistSystemsStatus.InService);
    }

    [Fact]
    public void Scenario_AssetRetiredAfterServiceLife_RejectsFurtherOperationalCommands()
    {
        var asset = NewAsset();
        asset.RecordLiftEvent(1000m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var liftAfterRetire = () => asset.RecordLiftEvent(500m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var qualifyAfterRetire = () => asset.QualifyOperator(TestUserId, FixedTimestamp, null, TestActor, TestCorrelationId, TestCausationId);
        var riggingAfterRetire = () => asset.RecordRiggingInspection(false, string.Empty, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        liftAfterRetire.Should().Throw<InvalidOperationException>();
        qualifyAfterRetire.Should().Throw<InvalidOperationException>();
        riggingAfterRetire.Should().Throw<InvalidOperationException>();
    }
}
