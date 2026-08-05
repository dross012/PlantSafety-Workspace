// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Commands
//  Layer:          Tests (Unit)
//  File:           SabotageIndicatorCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Scenario + Functional unit tests for RaiseSabotageIndicatorCommandHandler,
//                  exercising the module's "correlated multi-source indicators auto-escalate" and
//                  "sabotage indicators feed RiskManagement at elevated priority" risk hooks
//                  end-to-end through the handler (SabotageCorrelationService +
//                  SabotageRiskFeedPolicy), with the repository substituted.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Commands;

public sealed class RaiseSabotageIndicatorCommandHandlerTests : UnitTestBase
{
    private readonly ISabotageIndicatorRepository _repo = Substitute.For<ISabotageIndicatorRepository>();

    private RaiseSabotageIndicatorCommandHandler Sut() => new(_repo, NullLogger<RaiseSabotageIndicatorCommandHandler>());

    private static RaiseSabotageIndicatorCommand ValidCommand(string source = "ControlChange", string severity = "Medium") => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        source, "Unauthorized change detected", severity, null, TestUserId.Value);

    private static SabotageIndicatorAggregate OtherActiveIndicator(
        PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSource source) =>
        SabotageIndicatorAggregate.Raise(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            source: source, description: "other indicator",
            severity: PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSeverity.Medium);

    [Fact]
    public async Task Handle_WithNoCorrelatedIndicators_PersistsWithoutEscalating()
    {
        _repo.ListForCorrelationWindowAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AutoEscalated.Should().BeFalse();
        await _repo.Received(1).AddAsync(Arg.Any<SabotageIndicatorAggregate>(), Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<SabotageIndicatorAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCorrelatedIndicatorFromDistinctSource_AutoEscalatesAndPersistsUpdate()
    {
        _repo.ListForCorrelationWindowAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([OtherActiveIndicator(PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSource.SensorIntegrity)]);

        var result = await Sut().Handle(ValidCommand(source: "ControlChange"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AutoEscalated.Should().BeTrue();
        result.Value.Severity.Should().Be("Critical");
        await _repo.Received(1).UpdateAsync(Arg.Any<SabotageIndicatorAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExcludesTheNewIndicatorItselfFromCorrelationCandidates()
    {
        // The handler filters out its own just-persisted indicator from the candidate set before
        // correlating (see aggregate.SabotageIndicatorId != candidate filter) -- verify a single
        // same-source candidate returned by the repository does not accidentally double-count.
        SabotageIndicatorAggregate? addedIndicator = null;
        await _repo.AddAsync(Arg.Do<SabotageIndicatorAggregate>(a => addedIndicator = a), Arg.Any<CancellationToken>());

        _repo.ListForCorrelationWindowAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => addedIndicator is null ? [] : [addedIndicator]);

        var result = await Sut().Handle(ValidCommand(source: "ControlChange"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // With the self-reference filtered out, no genuine external correlation exists, so no
        // escalation should occur even though the repository stub echoed back a "candidate".
        result.Value.AutoEscalated.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUnrecognizedSeverity_DefaultsToMedium()
    {
        _repo.ListForCorrelationWindowAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut().Handle(ValidCommand(severity: "NotARealSeverity"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Severity.Should().Be("Medium");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.AddAsync(Arg.Any<SabotageIndicatorAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.SabotageIndicator.RaiseFailed");
    }
}
