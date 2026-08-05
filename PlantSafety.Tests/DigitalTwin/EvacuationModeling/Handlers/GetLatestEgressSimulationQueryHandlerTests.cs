// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           GetLatestEgressSimulationQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetLatestEgressSimulationQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class GetLatestEgressSimulationQueryHandlerTests : UnitTestBase
{
    private readonly IEgressSimulationRepository _repo = Substitute.For<IEgressSimulationRepository>();

    private GetLatestEgressSimulationQueryHandler Sut() => new(_repo, NullLogger<GetLatestEgressSimulationQueryHandler>());

    private static EgressSimulationItemDto SimulationFor(Guid zoneId) => new()
    {
        EgressSimulationRunId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        ZoneId = zoneId,
        TwinModelVersion = "v2026.1.0",
        TriggerReason = "Manual",
        SimulationStatus = "Completed",
        SimulatedEvacuationTimeSeconds = 305,
        PredictedPeakOccupancy = 42,
        StartedAtUtc = FixedUtcNow,
        CompletedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenLatestRunExists_ReturnsIt()
    {
        var zoneId = Guid.NewGuid();
        _repo.GetLatestByZoneAsync(TestTenantId.Value, null, zoneId, Arg.Any<CancellationToken>()).Returns(SimulationFor(zoneId));

        var result = await Sut().Handle(new GetLatestEgressSimulationQuery(TestTenantId.Value, null, zoneId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ZoneId.Should().Be(zoneId);
    }

    [Fact]
    public async Task Handle_WhenNoRunsExist_ReturnsSuccessWithNull()
    {
        _repo.GetLatestByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EgressSimulationItemDto?)null);

        var result = await Sut().Handle(new GetLatestEgressSimulationQuery(TestTenantId.Value, null, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.GetLatestByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<EgressSimulationItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetLatestEgressSimulationQuery(TestTenantId.Value, null, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.RetrievalFailed");
    }
}
