// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           RestrictedZonePolicyCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DefineRestrictedZonePolicyCommandHandler and
//                  ActivateRestrictedZonePolicyCommandHandler against a mocked
//                  IRestrictedZonePolicyRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using RestrictedZonePolicyAggregate = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class RestrictedZonePolicyCommandHandlerTests : UnitTestBase
{
    private readonly IRestrictedZonePolicyRepository _repository = Substitute.For<IRestrictedZonePolicyRepository>();

    private DefineRestrictedZonePolicyCommandHandler DefineSut() => new(_repository, NullLogger<DefineRestrictedZonePolicyCommandHandler>());
    private ActivateRestrictedZonePolicyCommandHandler ActivateSut() => new(_repository, NullLogger<ActivateRestrictedZonePolicyCommandHandler>());

    private static DefineRestrictedZonePolicyCommand ValidDefineCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        SafetyZoneId: Guid.NewGuid(),
        PolicyName: "High-Voltage Room Policy",
        AllowedAccessLevel: "RestrictedZoneA",
        EnforceAfterHoursLockout: true,
        PerformedByUserId: TestUserId.Value);

    // ---- Define --------------------------------------------------------------------

    [Fact]
    public async Task Define_WithValidCommand_ShouldPersistAggregate()
    {
        await DefineSut().Handle(ValidDefineCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<RestrictedZonePolicyAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Define_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await DefineSut().Handle(ValidDefineCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Define_WithValidCommand_ShouldReturnDraftStatus()
    {
        var result = await DefineSut().Handle(ValidDefineCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Define_ShouldPropagateAfterHoursLockoutFlagToAggregate()
    {
        RestrictedZonePolicyAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<RestrictedZonePolicyAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await DefineSut().Handle(ValidDefineCommand(), CancellationToken.None);

        captured!.EnforceAfterHoursLockout.Should().BeTrue();
    }

    [Fact]
    public async Task Define_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<RestrictedZonePolicyAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await DefineSut().Handle(ValidDefineCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.RestrictedZonePolicy.DefineFailed");
    }

    // ---- Activate ------------------------------------------------------------------

    private static ActivateRestrictedZonePolicyCommand ActivateCommand(Guid policyId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        RestrictedZonePolicyId: policyId,
        PerformedByUserId: TestUserId.Value);

    private static RestrictedZonePolicyAggregate DraftAggregate() =>
        RestrictedZonePolicyAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, safetyZoneId: Guid.NewGuid(), policyName: "Test Policy");

    [Fact]
    public async Task Activate_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = DraftAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids.RestrictedZonePolicyId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await ActivateSut().Handle(ActivateCommand(aggregate.RestrictedZonePolicyId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activate_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids.RestrictedZonePolicyId>(), Arg.Any<CancellationToken>())
            .Returns((RestrictedZonePolicyAggregate?)null);

        var result = await ActivateSut().Handle(ActivateCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.RestrictedZonePolicy.NotFound");
    }

    [Fact]
    public async Task Activate_WhenRetired_ShouldReturnFailureResult()
    {
        var aggregate = DraftAggregate();
        aggregate.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids.RestrictedZonePolicyId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await ActivateSut().Handle(ActivateCommand(aggregate.RestrictedZonePolicyId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.RestrictedZonePolicy.ActivateFailed");
    }
}
