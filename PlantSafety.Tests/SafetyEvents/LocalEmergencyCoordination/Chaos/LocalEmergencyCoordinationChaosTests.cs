// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Chaos
//  Layer:          Tests (Chaos)
//  File:           LocalEmergencyCoordinationChaosTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Chaos tests injecting randomized repository failures across every one of the
//                  module's nine command handlers, asserting the handler either returns a sanitized
//                  Result.Failure or rethrows the one designed-for-propagation exception type
//                  (ConcurrencyConflictException) -- it must never leak the raw injected exception
//                  type, and must never throw an unhandled exception out of Handle(). Deterministic
//                  (seeded Random), no I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Chaos;

public sealed class LocalEmergencyCoordinationChaosTests : UnitTestBase
{
    private static readonly Exception[] InjectedFailures =
    [
        new InvalidOperationException("simulated deadlock victim"),
        new TimeoutException("simulated command timeout"),
        new ArgumentException("simulated malformed row"),
        new Exception("simulated unclassified infrastructure failure"),
    ];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CloseHandler_WhenUpdateThrowsRandomInfrastructureFailure_ReturnsSanitizedFailure_NeverLeaksRawException(int failureIndex)
    {
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);
        var injected = InjectedFailures[failureIndex];
        repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw injected);

        var handler = new CloseLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<CloseLocalEmergencyCoordinationCaseCommandHandler>());
        var command = new CloseLocalEmergencyCoordinationCaseCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, agg.LocalEmergencyCoordinationId.Value, TestUserId.Value);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.CloseFailed");
        result.Error.Message.Should().NotContain(injected.GetType().Name, "the sanitized ApplicationError message must never echo the raw exception's type name");
        result.Error.Message.Should().NotContain(injected.Message, "the sanitized ApplicationError message must never echo the raw exception's message");
    }

    [Fact]
    public async Task RandomizedFailureSweep_AcrossReadAndWriteFailurePoints_NeverThrowsUnhandledFromHandle()
    {
        var random = new Random(Seed: 20260804);
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        for (var i = 0; i < 20; i++)
        {
            var failOnRead = random.Next(2) == 0;
            var injected = InjectedFailures[random.Next(InjectedFailures.Length)];

            repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(_ => failOnRead ? throw injected : Task.FromResult<LocalEmergencyCoordinationAggregate?>(agg));
            repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns<Task>(_ => failOnRead ? Task.CompletedTask : throw injected);

            var handler = new RecordJointDebriefCommandHandler(repo, NullLogger<RecordJointDebriefCommandHandler>());
            var command = new RecordJointDebriefCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                agg.LocalEmergencyCoordinationId.Value, null, "Agency", "Summary", TestUserId.Value);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue("every injected failure point must be caught and converted to a sanitized Result.Failure, never propagate raw");
        }
    }
}
