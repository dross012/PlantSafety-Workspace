// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Aggregates
//  Layer:          Tests (Unit)
//  File:           CraneHoistSystemsAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant unit tests for the CraneHoistSystems aggregate root,
//                  covering all 8 domain behaviors: Create (Register), RecordLiftEvent,
//                  RecordOverloadEvent, ScheduleCraneInspection, CompleteCraneInspection,
//                  QualifyOperator, SuspendOperatorQualification, RecordRiggingInspection, Retire.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Events;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Aggregates;

public sealed class CraneHoistSystemsAggregateTests : UnitTestBase
{
    private static Agg RegisteredAsset(decimal ratedLoadCapacityKg = 5000m) => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: null,
        assetTag: "CRANE-001",
        assetTitle: "Bay 3 Overhead Bridge Crane",
        assetType: CraneHoistAssetType.OverheadBridgeCrane,
        ratedLoadCapacityKg: ratedLoadCapacityKg);

    // ---- Create (Register) --------------------------------------------------

    [Fact]
    public void Create_WithValidData_SetsRegisteredStatusAndRaisesEvent()
    {
        var asset = RegisteredAsset();

        asset.Status.Should().Be(CraneHoistSystemsStatus.Registered);
        asset.AssetTag.Should().Be("CRANE-001");
        asset.AssetTitle.Should().Be("Bay 3 Overhead Bridge Crane");
        asset.AssetType.Should().Be(CraneHoistAssetType.OverheadBridgeCrane);
        asset.RatedLoadCapacityKg.Should().Be(5000m);
        asset.TenantId.Should().Be(TestTenantId.Value);
        asset.CreatedBy.Should().Be(TestUserId);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CraneHoistAssetRegisteredEvent>();
    }

    [Fact]
    public void Create_TrimsAssetTagAndTitle()
    {
        var asset = Agg.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
            assetTag: "  CRANE-002  ", assetTitle: "  Gantry  ", assetType: CraneHoistAssetType.GantryCrane,
            ratedLoadCapacityKg: 2500m);

        asset.AssetTag.Should().Be("CRANE-002");
        asset.AssetTitle.Should().Be("Gantry");
    }

    // ---- RecordLiftEvent -----------------------------------------------------

    [Fact]
    public void RecordLiftEvent_UpdatesObservedLoadAndRaisesEvent()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();

        asset.RecordLiftEvent(1200m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.LastObservedLoadKg.Should().Be(1200m);
        asset.LastLiftAtUtc.Should().Be(FixedTimestamp);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<LiftEventRecordedEvent>();
    }

    [Fact]
    public void RecordLiftEvent_OnRetiredAsset_Throws()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.RecordLiftEvent(500m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- RecordOverloadEvent --------------------------------------------------

    [Fact]
    public void RecordOverloadEvent_IncrementsCountAndFlagsOverload()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();

        asset.RecordOverloadEvent(6000m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.OverloadEventCount.Should().Be(1);
        asset.OverloadFlagged.Should().BeTrue();
        asset.LastObservedLoadKg.Should().Be(6000m);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<LoadLimitExceededEvent>();
    }

    [Fact]
    public void RecordOverloadEvent_CalledTwice_AccumulatesCount()
    {
        var asset = RegisteredAsset();

        asset.RecordOverloadEvent(6000m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.RecordOverloadEvent(6500m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.OverloadEventCount.Should().Be(2);
    }

    [Fact]
    public void RecordOverloadEvent_OnRetiredAsset_Throws()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.RecordOverloadEvent(6000m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- ScheduleCraneInspection -----------------------------------------------

    [Fact]
    public void ScheduleCraneInspection_FromRegistered_TransitionsToInspectionDue()
    {
        var asset = RegisteredAsset();
        var dueAt = UtcTimestamp.From(FixedUtcNow.AddDays(30));
        asset.ClearDomainEvents();

        asset.ScheduleCraneInspection(dueAt, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(CraneHoistSystemsStatus.InspectionDue);
        asset.InspectionDueAtUtc.Should().Be(dueAt);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CraneInspectionScheduledEvent>();
    }

    [Fact]
    public void ScheduleCraneInspection_FromRetired_Throws()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.ScheduleCraneInspection(FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- CompleteCraneInspection -----------------------------------------------

    [Fact]
    public void CompleteCraneInspection_WhenPassed_TransitionsToInService()
    {
        var asset = RegisteredAsset();
        asset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(30)), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.ClearDomainEvents();

        asset.CompleteCraneInspection(true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(CraneHoistSystemsStatus.InService);
        asset.LastInspectionPassed.Should().BeTrue();
        asset.LastInspectionAtUtc.Should().Be(FixedTimestamp);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CraneInspectionCompletedEvent>();
    }

    [Fact]
    public void CompleteCraneInspection_WhenFailed_TransitionsToInspectionOverdue()
    {
        var asset = RegisteredAsset();
        asset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(30)), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.CompleteCraneInspection(false, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(CraneHoistSystemsStatus.InspectionOverdue);
        asset.LastInspectionPassed.Should().BeFalse();
    }

    [Fact]
    public void CompleteCraneInspection_WhenCompletedAfterDueDate_AlsoRaisesOverdueEvent()
    {
        var asset = RegisteredAsset();
        var dueAt = UtcTimestamp.From(FixedUtcNow);
        asset.ScheduleCraneInspection(dueAt, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.ClearDomainEvents();

        var completedAt = UtcTimestamp.From(FixedUtcNow.AddDays(1));
        asset.CompleteCraneInspection(true, completedAt, TestActor, TestCorrelationId, TestCausationId);

        asset.DomainEvents.Should().HaveCount(2);
        asset.DomainEvents.Should().ContainItemsAssignableTo<ICraneHoistSystemsDomainEvent>();
        asset.DomainEvents.OfType<CraneInspectionOverdueEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CompleteCraneInspection_WithoutSchedule_Throws()
    {
        var asset = RegisteredAsset();

        var act = () => asset.CompleteCraneInspection(true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- QualifyOperator -----------------------------------------------

    [Fact]
    public void QualifyOperator_SetsOperatorFieldsAndClearsSuspension()
    {
        var asset = RegisteredAsset();
        asset.SuspendOperatorQualification("prior incident", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.ClearDomainEvents();

        var expiresAt = UtcTimestamp.From(FixedUtcNow.AddYears(1));
        asset.QualifyOperator(TestUserId, FixedTimestamp, expiresAt, TestActor, TestCorrelationId, TestCausationId);

        asset.OperatorUserId.Should().Be(TestUserId);
        asset.OperatorQualifiedAtUtc.Should().Be(FixedTimestamp);
        asset.OperatorQualificationExpiresAtUtc.Should().Be(expiresAt);
        asset.OperatorQualificationSuspended.Should().BeFalse();
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OperatorQualifiedEvent>();
    }

    [Fact]
    public void QualifyOperator_OnRetiredAsset_Throws()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.QualifyOperator(TestUserId, FixedTimestamp, null, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- SuspendOperatorQualification -----------------------------------------------

    [Fact]
    public void SuspendOperatorQualification_SetsSuspendedFlagAndRaisesEvent()
    {
        var asset = RegisteredAsset();
        asset.QualifyOperator(TestUserId, FixedTimestamp, null, TestActor, TestCorrelationId, TestCausationId);
        asset.ClearDomainEvents();

        asset.SuspendOperatorQualification("rigging defect found", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.OperatorQualificationSuspended.Should().BeTrue();
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OperatorQualificationSuspendedEvent>();
    }

    [Fact]
    public void SuspendOperatorQualification_TrimsReason()
    {
        var asset = RegisteredAsset();

        asset.SuspendOperatorQualification("  reason  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var evt = asset.DomainEvents.OfType<OperatorQualificationSuspendedEvent>().Single();
        evt.Reason.Should().Be("reason");
    }

    // ---- RecordRiggingInspection -----------------------------------------------

    [Fact]
    public void RecordRiggingInspection_WithNoDefect_UpdatesStateWithoutRaisingDefectEvent()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();

        asset.RecordRiggingInspection(false, "all clear", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.LastRiggingInspectionAtUtc.Should().Be(FixedTimestamp);
        asset.LastRiggingDefectDetected.Should().BeFalse();
        asset.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RecordRiggingInspection_WithDefect_RaisesDefectEvent()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();

        asset.RecordRiggingInspection(true, "frayed sling", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.LastRiggingDefectDetected.Should().BeTrue();
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RiggingDefectDetectedEvent>();
    }

    [Fact]
    public void RecordRiggingInspection_OnRetiredAsset_Throws()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.RecordRiggingInspection(false, string.Empty, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Retire -----------------------------------------------------------

    [Fact]
    public void Retire_FromAnyNonRetiredStatus_SetsRetiredStatus()
    {
        var asset = RegisteredAsset();

        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(CraneHoistSystemsStatus.Retired);
    }

    [Fact]
    public void Retire_CalledTwice_IsIdempotent()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().NotThrow();
        asset.Status.Should().Be(CraneHoistSystemsStatus.Retired);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var asset = RegisteredAsset();
        asset.DomainEvents.Should().NotBeEmpty();

        asset.ClearDomainEvents();

        asset.DomainEvents.Should().BeEmpty();
    }
}
