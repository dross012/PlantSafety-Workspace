// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Domain
//  Layer:          Tests (Unit)
//  File:           EvacuationZoneAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant unit tests for the EvacuationZone aggregate root
//                  (DigitalTwin.EvacuationModeling.EvacuationZones). Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Events;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Exceptions;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Domain;

/// <summary>Unit tests for <see cref="EvacuationZone"/> in <c>DigitalTwin.EvacuationModeling.EvacuationZones</c>.</summary>
public sealed class EvacuationZoneAggregateTests : UnitTestBase
{
    private static EvacuationZone CreateZone(int zoneCapacity = 200, int occupancyThreshold = 150) =>
        EvacuationZone.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            zoneName: "Production Floor A",
            twinModelVersion: "v2026.1.0",
            zoneCapacity: zoneCapacity,
            occupancyThreshold: occupancyThreshold);

    [Fact]
    public void Create_ShouldReturnAggregateWithActiveStatus()
    {
        var zone = CreateZone();
        zone.Status.Should().Be(EvacuationZoneStatus.Active);
    }

    [Fact]
    public void Create_ShouldRaiseDefinedEvent()
    {
        var zone = CreateZone();
        var evt = zone.DomainEvents.ShouldRaiseSingleEvent<EvacuationZoneDefinedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_WithBlankTwinModelVersion_ShouldThrow()
    {
        var act = () => EvacuationZone.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, twinModelVersion: "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldStartWithZeroCurrentOccupancy()
    {
        var zone = CreateZone();
        zone.CurrentOccupancyCount.Should().Be(0);
        zone.OccupancyReadings.Should().BeEmpty();
        zone.LastOccupancyReadingAtUtc.Should().BeNull();
    }

    [Fact]
    public void RecordOccupancySnapshot_BelowThreshold_ShouldStayActive()
    {
        var zone = CreateZone(zoneCapacity: 200, occupancyThreshold: 150);

        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 80, OccupancyReadingSource.Manual,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        zone.Status.Should().Be(EvacuationZoneStatus.Active);
        zone.CurrentOccupancyCount.Should().Be(80);
        zone.OccupancyReadings.Should().ContainSingle(r => !r.ExceededThreshold);
    }

    [Fact]
    public void RecordOccupancySnapshot_AboveThreshold_ShouldTransitionToBreachedAndRaiseBothEvents()
    {
        var zone = CreateZone(zoneCapacity: 200, occupancyThreshold: 150);
        zone.ClearDomainEvents();

        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 175, OccupancyReadingSource.BmsFeed,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        zone.Status.Should().Be(EvacuationZoneStatus.OccupancyThresholdBreached);
        zone.DomainEvents.Should().HaveCount(2);
        zone.DomainEvents.ShouldRaiseSingleEvent<OccupancySnapshotRecordedEvent>();
        var breach = zone.DomainEvents.ShouldRaiseSingleEvent<OccupancyThresholdExceededEvent>();
        breach.HeadcountEstimate.Should().Be(175);
        breach.ThresholdHeadcount.Should().Be(150);
    }

    [Fact]
    public void RecordOccupancySnapshot_WithZeroThreshold_ShouldNeverBreach()
    {
        // OccupancyThreshold == 0 means "alerting disabled" per RecordOccupancySnapshot's guard
        // (OccupancyThreshold > 0 && headcountEstimate > OccupancyThreshold).
        var zone = CreateZone(zoneCapacity: 200, occupancyThreshold: 0);

        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 500, OccupancyReadingSource.Sensor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        zone.Status.Should().Be(EvacuationZoneStatus.Active);
    }

    [Fact]
    public void RecordOccupancySnapshot_ReturningBelowThreshold_ShouldReturnToActive()
    {
        var zone = CreateZone(zoneCapacity: 200, occupancyThreshold: 150);
        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 175, OccupancyReadingSource.Manual,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        zone.Status.Should().Be(EvacuationZoneStatus.OccupancyThresholdBreached);

        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 90, OccupancyReadingSource.Manual,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        zone.Status.Should().Be(EvacuationZoneStatus.Active);
        zone.OccupancyReadings.Should().HaveCount(2);
    }

    [Fact]
    public void RecordOccupancySnapshot_WithNegativeHeadcount_ShouldThrow()
    {
        var zone = CreateZone();

        var act = () => zone.RecordOccupancySnapshot(OccupancyReadingId.New(), -1, OccupancyReadingSource.Manual,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RecordOccupancySnapshot_ShouldUpdateLastOccupancyReadingAtUtc()
    {
        var zone = CreateZone();
        var readingAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(3));

        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 10, OccupancyReadingSource.Manual,
            readingAt, TestActor, TestCorrelationId, TestCausationId);

        zone.LastOccupancyReadingAtUtc.Should().Be(readingAt);
    }

    [Fact]
    public void Retire_OnActiveZone_ShouldTransitionToRetired()
    {
        var zone = CreateZone();
        zone.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        zone.Status.Should().Be(EvacuationZoneStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseRetiredEvent()
    {
        var zone = CreateZone();
        zone.ClearDomainEvents();

        zone.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        zone.DomainEvents.ShouldRaiseSingleEvent<EvacuationZoneRetiredEvent>();
    }

    [Fact]
    public void Retire_AlreadyRetired_ShouldThrow()
    {
        var zone = CreateZone();
        zone.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => zone.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<EvacuationZoneInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var zone = CreateZone();
        zone.ClearDomainEvents();
        zone.DomainEvents.Should().BeEmpty();
    }
}
