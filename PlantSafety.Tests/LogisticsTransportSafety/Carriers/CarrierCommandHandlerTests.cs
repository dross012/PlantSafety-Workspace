// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.Carriers
//  Layer:          Tests (Unit)
//  File:           CarrierCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all Carrier command handlers: Register, Qualify, Suspend,
//                  Retire. Repository is mocked with NSubstitute; no I/O, no network, <50ms per
//                  test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier;

namespace PlantSafety.Tests.LogisticsTransportSafety.Carriers;

public sealed class CarrierCommandHandlerTests : UnitTestBase
{
    private readonly ICarrierRepository _repo = Substitute.For<ICarrierRepository>();

    private static Agg PendingCarrier() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, carrierName: "Acme Freight Lines");

    // ---- RegisterCarrierCommandHandler --------------------------------------------

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new RegisterCarrierCommandHandler(_repo, NullLogger<RegisterCarrierCommandHandler>());

        var result = await sut.Handle(new RegisterCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Acme Freight Lines", "ThirdPartyContractCarrier", null, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("PendingQualification");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithInvalidCarrierType_ShouldReturnFailure()
    {
        var sut = new RegisterCarrierCommandHandler(_repo, NullLogger<RegisterCarrierCommandHandler>());

        var result = await sut.Handle(new RegisterCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Acme Freight Lines", "NotARealType", null, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.InvalidCarrierType");
    }

    // ---- QualifyCarrierCommandHandler ---------------------------------------------

    [Fact]
    public async Task Qualify_WhenAggregateExists_ShouldReturnSuccess()
    {
        var carrier = PendingCarrier();
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns(carrier);
        var sut = new QualifyCarrierCommandHandler(_repo, NullLogger<QualifyCarrierCommandHandler>());

        var result = await sut.Handle(new QualifyCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            carrier.CarrierId.Value, 91m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Qualified");
        await _repo.Received(1).UpdateAsync(carrier, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Qualify_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new QualifyCarrierCommandHandler(_repo, NullLogger<QualifyCarrierCommandHandler>());

        var result = await sut.Handle(new QualifyCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), 91m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.NotFound");
    }

    [Fact]
    public async Task Qualify_WhenDomainRejectsScore_ShouldReturnGenericFailure()
    {
        var carrier = PendingCarrier();
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns(carrier);
        var sut = new QualifyCarrierCommandHandler(_repo, NullLogger<QualifyCarrierCommandHandler>());

        var result = await sut.Handle(new QualifyCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            carrier.CarrierId.Value, -5m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.QualifyFailed");
    }

    // ---- SuspendCarrierCommandHandler ----------------------------------------------

    [Fact]
    public async Task Suspend_WhenAggregateExists_ShouldReturnSuccess()
    {
        var carrier = PendingCarrier();
        carrier.Qualify(90m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns(carrier);
        var sut = new SuspendCarrierCommandHandler(_repo, NullLogger<SuspendCarrierCommandHandler>());

        var result = await sut.Handle(new SuspendCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            carrier.CarrierId.Value, "Insurance lapsed", TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Suspended");
    }

    [Fact]
    public async Task Suspend_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new SuspendCarrierCommandHandler(_repo, NullLogger<SuspendCarrierCommandHandler>());

        var result = await sut.Handle(new SuspendCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "reason", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.NotFound");
    }

    // ---- RetireCarrierCommandHandler ------------------------------------------------

    [Fact]
    public async Task Retire_WhenAggregateExists_ShouldReturnSuccess()
    {
        var carrier = PendingCarrier();
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns(carrier);
        var sut = new RetireCarrierCommandHandler(_repo, NullLogger<RetireCarrierCommandHandler>());

        var result = await sut.Handle(new RetireCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            carrier.CarrierId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Retired");
        await _repo.Received(1).UpdateAsync(carrier, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retire_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<CarrierId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RetireCarrierCommandHandler(_repo, NullLogger<RetireCarrierCommandHandler>());

        var result = await sut.Handle(new RetireCarrierCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.NotFound");
    }
}
