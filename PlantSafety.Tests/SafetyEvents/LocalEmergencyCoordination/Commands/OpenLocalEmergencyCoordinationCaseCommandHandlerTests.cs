// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Commands
//  Layer:          Tests (Unit)
//  File:           OpenLocalEmergencyCoordinationCaseCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for OpenLocalEmergencyCoordinationCaseCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Commands;

public sealed class OpenLocalEmergencyCoordinationCaseCommandHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private OpenLocalEmergencyCoordinationCaseCommandHandler Sut() => new(_repo, NullLogger<OpenLocalEmergencyCoordinationCaseCommandHandler>());

    private static OpenLocalEmergencyCoordinationCaseCommand Command() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow,
        ApiVersionToken: "v1",
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        CaseName: "Wildfire Mutual Aid Case",
        OpenedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsCreatedStatus()
    {
        var result = await Sut().Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Created");
        result.Value.LocalEmergencyCoordinationId.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.OpenFailed");
    }
}
