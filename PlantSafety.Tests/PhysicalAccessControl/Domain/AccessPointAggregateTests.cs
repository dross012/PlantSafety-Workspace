// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessPointAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the AccessPoint aggregate root: full device lifecycle
//                  (Register -> Activate -> ScheduleMaintenance -> Decommission), invariant-guard
//                  tests for illegal transitions, and domain-event assertions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="AccessPoint"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class AccessPointAggregateTests : UnitTestBase
{
    private static AccessPoint CreateRegistered(
        AccessPointDeviceType deviceType = AccessPointDeviceType.BadgeReader,
        Guid? safetyZoneId = null,
        bool isPerimeter = false) =>
        AccessPoint.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accessPointCode: "AP-001",
            description: "Main lobby turnstile",
            deviceType: deviceType,
            safetyZoneId: safetyZoneId,
            isPerimeter: isPerimeter);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateRegistered().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusRegistered()
    {
        var agg = CreateRegistered();
        agg.Status.Should().Be(AccessPointStatus.Registered);
    }

    [Fact]
    public void Create_ShouldTrimCodeAndDescription()
    {
        var agg = AccessPoint.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accessPointCode: "  AP-002  ",
            description: "  Dock door  ");

        agg.AccessPointCode.Should().Be("AP-002");
        agg.Description.Should().Be("Dock door");
    }

    [Fact]
    public void Create_ShouldRaiseAccessPointRegisteredEvent()
    {
        var agg = CreateRegistered();
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessPointRegisteredEvent>();
    }

    [Fact]
    public void Create_WithPerimeterFlag_ShouldPersistFlag()
    {
        var agg = CreateRegistered(isPerimeter: true);
        agg.IsPerimeter.Should().BeTrue();
    }

    [Fact]
    public void Create_WithSafetyZoneId_ShouldPersistLinkage()
    {
        var zoneId = Guid.NewGuid();
        var agg = CreateRegistered(safetyZoneId: zoneId);
        agg.SafetyZoneId.Should().Be(zoneId);
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateRegistered();
        var a2 = CreateRegistered();
        a1.AccessPointId.Should().NotBe(a2.AccessPointId);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- Activate ----------------------------------------------------------------

    [Fact]
    public void Activate_WhenRegistered_ShouldTransitionToActive()
    {
        var agg = CreateRegistered();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccessPointStatus.Active);
    }

    [Fact]
    public void Activate_WhenRegistered_ShouldRaiseEvent()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessPointActivatedEvent>();
    }

    [Fact]
    public void Activate_WhenDecommissioned_ShouldThrowInvalidOperationException()
    {
        var agg = CreateRegistered();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- ScheduleMaintenance -------------------------------------------------------

    [Fact]
    public void ScheduleMaintenance_WhenActive_ShouldTransitionToMaintenance()
    {
        var agg = CreateRegistered();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessPointStatus.Maintenance);
    }

    [Fact]
    public void ScheduleMaintenance_WhenActive_ShouldRaiseEvent()
    {
        var agg = CreateRegistered();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<AccessPointMaintenanceScheduledEvent>();
    }

    [Fact]
    public void ScheduleMaintenance_WhenRegistered_ShouldThrowInvalidOperationException()
    {
        var agg = CreateRegistered();
        var act = () => agg.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Activate_FromMaintenance_ShouldTransitionToActive()
    {
        var agg = CreateRegistered();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessPointStatus.Active);
    }

    // ---- Decommission --------------------------------------------------------------

    [Theory]
    [InlineData(AccessPointStatus.Registered)]
    [InlineData(AccessPointStatus.Active)]
    [InlineData(AccessPointStatus.Maintenance)]
    public void Decommission_FromAnyNonTerminalState_ShouldTransitionToDecommissioned(AccessPointStatus fromStatus)
    {
        var agg = CreateRegistered();
        if (fromStatus is AccessPointStatus.Active or AccessPointStatus.Maintenance)
            agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        if (fromStatus is AccessPointStatus.Maintenance)
            agg.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessPointStatus.Decommissioned);
    }

    [Fact]
    public void Decommission_ShouldRaiseEvent()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessPointDecommissionedEvent>();
    }

    [Fact]
    public void Decommission_WhenAlreadyDecommissioned_ShouldThrowInvalidOperationException()
    {
        var agg = CreateRegistered();
        agg.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
