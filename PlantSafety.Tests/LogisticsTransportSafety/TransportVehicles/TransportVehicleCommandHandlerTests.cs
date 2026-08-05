// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportVehicles
//  Layer:          Tests (Unit)
//  File:           TransportVehicleCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all TransportVehicle command handlers: Register,
//                  ScheduleInspection, RecordInspectionResult, Retire. Repository is mocked with
//                  NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportVehicles;

public sealed class TransportVehicleCommandHandlerTests : UnitTestBase
{
    private readonly ITransportVehicleRepository _repo = Substitute.For<ITransportVehicleRepository>();

    private static Agg RegisteredVehicle() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleTitle: "Forklift 12", vehicleIdentifier: "FLT-012");

    // ---- RegisterTransportVehicleCommandHandler ---------------------------------

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new RegisterTransportVehicleCommandHandler(_repo, NullLogger<RegisterTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RegisterTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Forklift 12", "Forklift", "FLT-012", TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Registered");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithInvalidVehicleType_ShouldReturnFailure()
    {
        var sut = new RegisterTransportVehicleCommandHandler(_repo, NullLogger<RegisterTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RegisterTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Forklift 12", "NotARealType", "FLT-012", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.InvalidVehicleType");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    // ---- ScheduleInspectionTransportVehicleCommandHandler -----------------------

    [Fact]
    public async Task ScheduleInspection_WhenAggregateExists_ShouldReturnSuccessAndCallUpdateAsync()
    {
        var vehicle = RegisteredVehicle();
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns(vehicle);
        var sut = new ScheduleInspectionTransportVehicleCommandHandler(_repo, NullLogger<ScheduleInspectionTransportVehicleCommandHandler>());

        var result = await sut.Handle(new ScheduleInspectionTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            vehicle.TransportVehicleId.Value, FixedUtcNow.AddDays(7), TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("InspectionScheduled");
        await _repo.Received(1).UpdateAsync(vehicle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScheduleInspection_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new ScheduleInspectionTransportVehicleCommandHandler(_repo, NullLogger<ScheduleInspectionTransportVehicleCommandHandler>());

        var result = await sut.Handle(new ScheduleInspectionTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), FixedUtcNow.AddDays(7), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.NotFound");
    }

    // ---- RecordInspectionResultTransportVehicleCommandHandler --------------------

    [Fact]
    public async Task RecordInspectionResult_WhenAggregateExists_ShouldReturnSuccess()
    {
        var vehicle = RegisteredVehicle();
        vehicle.ScheduleInspection(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns(vehicle);
        var sut = new RecordInspectionResultTransportVehicleCommandHandler(_repo, NullLogger<RecordInspectionResultTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RecordInspectionResultTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            vehicle.TransportVehicleId.Value, true, 92m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("InspectionPassed");
        await _repo.Received(1).UpdateAsync(vehicle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordInspectionResult_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RecordInspectionResultTransportVehicleCommandHandler(_repo, NullLogger<RecordInspectionResultTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RecordInspectionResultTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), true, 92m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.NotFound");
    }

    [Fact]
    public async Task RecordInspectionResult_WhenDomainThrows_ShouldReturnGenericFailure()
    {
        var vehicle = RegisteredVehicle(); // no inspection scheduled -> domain will throw
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns(vehicle);
        var sut = new RecordInspectionResultTransportVehicleCommandHandler(_repo, NullLogger<RecordInspectionResultTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RecordInspectionResultTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            vehicle.TransportVehicleId.Value, true, 92m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.RecordInspectionResultFailed");
    }

    // ---- RetireTransportVehicleCommandHandler -------------------------------------

    [Fact]
    public async Task Retire_WhenAggregateExists_ShouldReturnSuccess()
    {
        var vehicle = RegisteredVehicle();
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns(vehicle);
        var sut = new RetireTransportVehicleCommandHandler(_repo, NullLogger<RetireTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RetireTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            vehicle.TransportVehicleId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Retired");
        await _repo.Received(1).UpdateAsync(vehicle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retire_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportVehicleId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RetireTransportVehicleCommandHandler(_repo, NullLogger<RetireTransportVehicleCommandHandler>());

        var result = await sut.Handle(new RetireTransportVehicleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.NotFound");
    }
}
