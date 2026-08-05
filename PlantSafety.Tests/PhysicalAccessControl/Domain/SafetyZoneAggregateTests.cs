// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           SafetyZoneAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the SafetyZone aggregate root: registration, restriction,
//                  deactivation, and occupancy-correlation lifecycle, invariant-guard tests for
//                  illegal transitions and the negative-occupancy guard, and domain-event
//                  assertions. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="SafetyZone"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class SafetyZoneAggregateTests : UnitTestBase
{
    private static SafetyZone CreateActive(
        SafetyZoneHazardLevel hazardLevel = SafetyZoneHazardLevel.Medium,
        int maxOccupancy = 25) =>
        SafetyZone.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            zoneCode: "ZONE-A",
            zoneName: "Production Floor A",
            hazardLevel: hazardLevel,
            maxOccupancy: maxOccupancy);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateActive().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusActive()
    {
        var agg = CreateActive();
        agg.Status.Should().Be(SafetyZoneStatus.Active);
    }

    [Fact]
    public void Create_ShouldSetCurrentOccupancyToZero()
    {
        var agg = CreateActive();
        agg.CurrentOccupancy.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldRaiseSafetyZoneRegisteredEvent()
    {
        var agg = CreateActive();
        agg.DomainEvents.ShouldRaiseSingleEvent<SafetyZoneRegisteredEvent>();
    }

    [Theory]
    [InlineData(SafetyZoneHazardLevel.Low)]
    [InlineData(SafetyZoneHazardLevel.Medium)]
    [InlineData(SafetyZoneHazardLevel.High)]
    [InlineData(SafetyZoneHazardLevel.Critical)]
    public void Create_WithHazardLevel_ShouldPersistClassification(SafetyZoneHazardLevel hazardLevel)
    {
        var agg = CreateActive(hazardLevel);
        agg.HazardLevel.Should().Be(hazardLevel);
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateActive();
        var a2 = CreateActive();
        a1.SafetyZoneId.Should().NotBe(a2.SafetyZoneId);
    }

    // ---- Restrict ------------------------------------------------------------------

    [Fact]
    public void Restrict_WhenActive_ShouldTransitionToRestricted()
    {
        var agg = CreateActive();
        agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(SafetyZoneStatus.Restricted);
    }

    [Fact]
    public void Restrict_ShouldRaiseEvent()
    {
        var agg = CreateActive();
        agg.ClearDomainEvents();
        agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<SafetyZoneRestrictedEvent>();
    }

    [Fact]
    public void Restrict_WhenAlreadyRestricted_ShouldThrowInvalidOperationException()
    {
        var agg = CreateActive();
        agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Deactivate ------------------------------------------------------------------

    [Fact]
    public void Deactivate_WhenActive_ShouldTransitionToInactive()
    {
        var agg = CreateActive();
        agg.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(SafetyZoneStatus.Inactive);
    }

    [Fact]
    public void Deactivate_FromRestricted_ShouldTransitionToInactive()
    {
        var agg = CreateActive();
        agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(SafetyZoneStatus.Inactive);
    }

    [Fact]
    public void Deactivate_ShouldRaiseEvent()
    {
        var agg = CreateActive();
        agg.ClearDomainEvents();
        agg.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<SafetyZoneDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowInvalidOperationException()
    {
        var agg = CreateActive();
        agg.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- UpdateOccupancy -------------------------------------------------------------

    [Fact]
    public void UpdateOccupancy_WithValidValue_ShouldSetCurrentOccupancy()
    {
        var agg = CreateActive();
        agg.UpdateOccupancy(12, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CurrentOccupancy.Should().Be(12);
    }

    [Fact]
    public void UpdateOccupancy_ShouldRaiseEvent()
    {
        var agg = CreateActive();
        agg.ClearDomainEvents();
        agg.UpdateOccupancy(5, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<SafetyZoneOccupancyUpdatedEvent>();
    }

    [Fact]
    public void UpdateOccupancy_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        var agg = CreateActive();
        var act = () => agg.UpdateOccupancy(-1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateOccupancy_WithZero_ShouldSucceed()
    {
        var agg = CreateActive();
        agg.UpdateOccupancy(0, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CurrentOccupancy.Should().Be(0);
    }

    [Fact]
    public void UpdateOccupancy_WhenRestricted_ShouldStillBeAllowed()
    {
        // Occupancy correlation runs independently of the restriction lifecycle state.
        var agg = CreateActive();
        agg.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.UpdateOccupancy(3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.CurrentOccupancy.Should().Be(3);
    }
}
