// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           DischargePointCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the DischargePoint aggregate: Register, Deactivate,
//                  Reactivate, Decommission.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class DischargePointCommandHandlerTests : UnitTestBase
{
    private readonly IDischargePointRepository _repo = Substitute.For<IDischargePointRepository>();

    private static Agg RegisteredPoint() => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        dischargePermitId: Guid.NewGuid(), pointCode: "Outfall 001", pointName: "Main Outfall",
        receivingWaterBody: "Willow Creek", outfall: OutfallType.Direct);

    [Fact]
    public async Task Register_WithValidCommand_PersistsNewPointAsActive()
    {
        var sut = new RegisterDischargePointCommandHandler(_repo, NullLogger<RegisterDischargePointCommandHandler>());
        var command = new RegisterDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), "Outfall 002", "Secondary Outfall", "Willow Creek", "Direct", null, null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePointStatus.Active));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_WhenActive_TransitionsToInactiveAndPersists()
    {
        var point = RegisteredPoint();
        _repo.GetByIdAsync(Arg.Any<DischargePointId>(), Arg.Any<CancellationToken>()).Returns(point);
        var sut = new DeactivateDischargePointCommandHandler(_repo, NullLogger<DeactivateDischargePointCommandHandler>());
        var command = new DeactivateDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            point.DischargePointId.Value, "Seasonal shutdown.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePointStatus.Inactive));
        await _repo.Received(1).UpdateAsync(point, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_WhenPointDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DischargePointId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new DeactivateDischargePointCommandHandler(_repo, NullLogger<DeactivateDischargePointCommandHandler>());
        var command = new DeactivateDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Reactivate_WhenInactive_TransitionsToActive()
    {
        var point = RegisteredPoint();
        point.Deactivate(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargePointId>(), Arg.Any<CancellationToken>()).Returns(point);
        var sut = new ReactivateDischargePointCommandHandler(_repo, NullLogger<ReactivateDischargePointCommandHandler>());
        var command = new ReactivateDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            point.DischargePointId.Value, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePointStatus.Active));
    }

    [Fact]
    public async Task Decommission_WhenActive_TransitionsToDecommissioned()
    {
        var point = RegisteredPoint();
        _repo.GetByIdAsync(Arg.Any<DischargePointId>(), Arg.Any<CancellationToken>()).Returns(point);
        var sut = new DecommissionDischargePointCommandHandler(_repo, NullLogger<DecommissionDischargePointCommandHandler>());
        var command = new DecommissionDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            point.DischargePointId.Value, "End of life.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePointStatus.Decommissioned));
    }

    [Fact]
    public async Task Decommission_WhenAlreadyDecommissioned_ReturnsFailureRatherThanThrowing()
    {
        var point = RegisteredPoint();
        point.Decommission(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargePointId>(), Arg.Any<CancellationToken>()).Returns(point);
        var sut = new DecommissionDischargePointCommandHandler(_repo, NullLogger<DecommissionDischargePointCommandHandler>());
        var command = new DecommissionDischargePointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            point.DischargePointId.Value, null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
