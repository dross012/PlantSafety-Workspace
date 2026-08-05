// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessPointCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterAccessPointCommandHandler and
//                  DecommissionAccessPointCommandHandler against a mocked IAccessPointRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using AccessPointAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class AccessPointCommandHandlerTests : UnitTestBase
{
    private readonly IAccessPointRepository _repository = Substitute.For<IAccessPointRepository>();

    private RegisterAccessPointCommandHandler RegisterSut() => new(_repository, NullLogger<RegisterAccessPointCommandHandler>());
    private DecommissionAccessPointCommandHandler DecommissionSut() => new(_repository, NullLogger<DecommissionAccessPointCommandHandler>());

    private static RegisterAccessPointCommand ValidRegisterCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AccessPointCode: "AP-100",
        Description: "Loading dock reader",
        DeviceType: "BadgeReader",
        SafetyZoneId: null,
        IsPerimeter: false,
        PerformedByUserId: TestUserId.Value);

    // ---- Register ----------------------------------------------------------------

    [Fact]
    public async Task Register_WithValidCommand_ShouldPersistAggregate()
    {
        await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<AccessPointAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnRegisteredStatus()
    {
        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Registered");
    }

    [Fact]
    public async Task Register_WithUnknownDeviceType_ShouldDefaultToBadgeReader()
    {
        var command = ValidRegisterCommand() with { DeviceType = "NotARealDeviceType" };

        AccessPointAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<AccessPointAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await RegisterSut().Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.DeviceType.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointDeviceType.BadgeReader);
    }

    [Fact]
    public async Task Register_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<AccessPointAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessPoint.RegisterFailed");
    }

    // ---- Decommission --------------------------------------------------------------

    private static DecommissionAccessPointCommand DecommissionCommand(Guid accessPointId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        AccessPointId: accessPointId,
        PerformedByUserId: TestUserId.Value);

    private static AccessPointAggregate RegisteredAggregate() =>
        AccessPointAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accessPointCode: "AP-200", description: "Gate");

    [Fact]
    public async Task Decommission_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = RegisteredAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids.AccessPointId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await DecommissionSut().Handle(DecommissionCommand(aggregate.AccessPointId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Decommissioned");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Decommission_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids.AccessPointId>(), Arg.Any<CancellationToken>())
            .Returns((AccessPointAggregate?)null);

        var result = await DecommissionSut().Handle(DecommissionCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessPoint.NotFound");
    }

    [Fact]
    public async Task Decommission_WhenAlreadyDecommissioned_ShouldReturnFailureResult()
    {
        var aggregate = RegisteredAggregate();
        aggregate.Decommission(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids.AccessPointId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await DecommissionSut().Handle(DecommissionCommand(aggregate.AccessPointId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessPoint.DecommissionFailed");
    }
}
