// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           RecordOccupancySnapshotCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordOccupancySnapshotCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class RecordOccupancySnapshotCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationZoneRepository _repo = Substitute.For<IEvacuationZoneRepository>();

    private RecordOccupancySnapshotCommandHandler Sut() => new(_repo, NullLogger<RecordOccupancySnapshotCommandHandler>());

    private static EvacuationZone CreatedZone(int threshold = 150) => EvacuationZone.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        tenantId: TestTenantId.Value, zoneName: "Zone A", twinModelVersion: "v2026.1.0",
        zoneCapacity: 200, occupancyThreshold: threshold);

    [Fact]
    public async Task Handle_WithExistingZone_ReturnsSuccessAndRecordsSnapshot()
    {
        var zone = CreatedZone();
        _repo.GetByIdAsync(zone.EvacuationZoneId, Arg.Any<CancellationToken>()).Returns(zone);

        var command = new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            zone.EvacuationZoneId.Value, 80, "BmsFeed", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        zone.OccupancyReadings.Should().ContainSingle(r => r.HeadcountEstimate == 80);
        await _repo.Received(1).UpdateAsync(zone, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AboveThreshold_ReturnsBreachedStatus()
    {
        var zone = CreatedZone(threshold: 50);
        _repo.GetByIdAsync(zone.EvacuationZoneId, Arg.Any<CancellationToken>()).Returns(zone);

        var command = new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            zone.EvacuationZoneId.Value, 90, "Manual", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.Value.Status.Should().Be("OccupancyThresholdBreached");
    }

    [Fact]
    public async Task Handle_WithUnknownSourceString_DefaultsToManual()
    {
        var zone = CreatedZone();
        _repo.GetByIdAsync(zone.EvacuationZoneId, Arg.Any<CancellationToken>()).Returns(zone);

        var command = new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            zone.EvacuationZoneId.Value, 10, "NotARealSource", TestUserId.Value);

        await Sut().Handle(command, CancellationToken.None);

        zone.OccupancyReadings.Single().Source.Should()
            .Be(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums.OccupancyReadingSource.Manual);
    }

    [Fact]
    public async Task Handle_WhenZoneNotFound_ReturnsFailureWithNotFoundCode()
    {
        _repo.GetByIdAsync(Arg.Any<EvacuationZoneId>(), Arg.Any<CancellationToken>()).Returns((EvacuationZone?)null);

        var command = new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), 10, "Manual", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationZone.NotFound");
    }

    [Fact]
    public async Task Handle_WithNegativeHeadcount_ReturnsFailureWithoutPropagatingException()
    {
        var zone = CreatedZone();
        _repo.GetByIdAsync(zone.EvacuationZoneId, Arg.Any<CancellationToken>()).Returns(zone);

        var command = new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            zone.EvacuationZoneId.Value, -5, "Manual", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationZone.RecordOccupancyFailed");
    }
}
