// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           SafetyZoneCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterSafetyZoneCommandHandler and
//                  RestrictSafetyZoneCommandHandler against a mocked ISafetyZoneRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using SafetyZoneAggregate = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class SafetyZoneCommandHandlerTests : UnitTestBase
{
    private readonly ISafetyZoneRepository _repository = Substitute.For<ISafetyZoneRepository>();

    private RegisterSafetyZoneCommandHandler RegisterSut() => new(_repository, NullLogger<RegisterSafetyZoneCommandHandler>());
    private RestrictSafetyZoneCommandHandler RestrictSut() => new(_repository, NullLogger<RestrictSafetyZoneCommandHandler>());

    private static RegisterSafetyZoneCommand ValidRegisterCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        ZoneCode: "ZONE-B",
        ZoneName: "Chemical Storage",
        HazardLevel: "High",
        MaxOccupancy: 10,
        PerformedByUserId: TestUserId.Value);

    // ---- Register ------------------------------------------------------------------

    [Fact]
    public async Task Register_WithValidCommand_ShouldPersistAggregate()
    {
        await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<SafetyZoneAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnActiveStatus()
    {
        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Register_WithUnknownHazardLevel_ShouldDefaultToLow()
    {
        var command = ValidRegisterCommand() with { HazardLevel = "NotARealLevel" };

        SafetyZoneAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<SafetyZoneAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await RegisterSut().Handle(command, CancellationToken.None);

        captured!.HazardLevel.Should().Be(PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums.SafetyZoneHazardLevel.Low);
    }

    [Fact]
    public async Task Register_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<SafetyZoneAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await RegisterSut().Handle(ValidRegisterCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.SafetyZone.RegisterFailed");
    }

    // ---- Restrict ------------------------------------------------------------------

    private static RestrictSafetyZoneCommand RestrictCommand(Guid safetyZoneId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SafetyZoneId: safetyZoneId,
        PerformedByUserId: TestUserId.Value);

    private static SafetyZoneAggregate ActiveAggregate() =>
        SafetyZoneAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneCode: "ZONE-C", zoneName: "Test Zone", maxOccupancy: 5);

    [Fact]
    public async Task Restrict_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = ActiveAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids.SafetyZoneId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await RestrictSut().Handle(RestrictCommand(aggregate.SafetyZoneId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Restricted");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Restrict_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids.SafetyZoneId>(), Arg.Any<CancellationToken>())
            .Returns((SafetyZoneAggregate?)null);

        var result = await RestrictSut().Handle(RestrictCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.SafetyZone.NotFound");
    }

    [Fact]
    public async Task Restrict_WhenAlreadyRestricted_ShouldReturnFailureResult()
    {
        var aggregate = ActiveAggregate();
        aggregate.Restrict(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids.SafetyZoneId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await RestrictSut().Handle(RestrictCommand(aggregate.SafetyZoneId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.SafetyZone.RestrictFailed");
    }
}
