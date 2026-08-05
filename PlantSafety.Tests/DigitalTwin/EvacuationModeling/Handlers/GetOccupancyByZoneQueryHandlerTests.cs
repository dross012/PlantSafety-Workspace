// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           GetOccupancyByZoneQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetOccupancyByZoneQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class GetOccupancyByZoneQueryHandlerTests : UnitTestBase
{
    private readonly IEvacuationZoneRepository _repo = Substitute.For<IEvacuationZoneRepository>();

    private GetOccupancyByZoneQueryHandler Sut() => new(_repo, NullLogger<GetOccupancyByZoneQueryHandler>());

    private static OccupancyReadingItemDto ReadingFor(Guid zoneId) => new()
    {
        OccupancyReadingId = Guid.NewGuid(),
        EvacuationZoneId = zoneId,
        HeadcountEstimate = 80,
        Source = "Manual",
        RecordedAtUtc = FixedUtcNow,
        ExceededThreshold = false,
    };

    [Fact]
    public async Task Handle_WithReadingsInRange_ReturnsThem()
    {
        var zoneId = Guid.NewGuid();
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;
        _repo.GetOccupancyByZoneAsync(TestTenantId.Value, null, zoneId, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<OccupancyReadingItemDto> { ReadingFor(zoneId) });

        var result = await Sut().Handle(new GetOccupancyByZoneQuery(TestTenantId.Value, null, zoneId, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(r => r.EvacuationZoneId == zoneId);
    }

    [Fact]
    public async Task Handle_WithNoReadingsInRange_ReturnsEmptyList()
    {
        _repo.GetOccupancyByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<OccupancyReadingItemDto>());

        var result = await Sut().Handle(new GetOccupancyByZoneQuery(TestTenantId.Value, null, Guid.NewGuid(), FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.GetOccupancyByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<OccupancyReadingItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetOccupancyByZoneQuery(TestTenantId.Value, null, Guid.NewGuid(), FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationZone.OccupancyRetrievalFailed");
    }
}
