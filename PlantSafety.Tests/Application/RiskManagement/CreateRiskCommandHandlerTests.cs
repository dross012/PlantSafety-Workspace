// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.RiskManagement
//  Layer:          Tests (Unit)
//  File:           CreateRiskCommandHandlerTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateRiskCommandHandler with a mocked IRiskRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.Domain.RiskManagement.Interfaces;
using RiskAggregate = PlantSafety.Domain.RiskManagement.Aggregates.Risk;

namespace PlantSafety.Tests.Application.RiskManagement;

public sealed class CreateRiskCommandHandlerTests : UnitTestBase
{
    private readonly IRiskRepository _repository = Substitute.For<IRiskRepository>();

    private CreateRiskCommandHandler Sut() => new(_repository, NullLogger<CreateRiskCommandHandler>());

    private static CreateRiskCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow,
        ApiVersionToken: "v1",
        TenantId: TestTenantId.Value,
        SiteId: null,
        RiskTitle: "Unguarded conveyor pinch point",
        Location: "Line 3",
        HazardSource: "Mechanical",
        OwnerId: TestUserId.Value,
        ReviewRequired: true);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAggregate()
    {
        await Sut().Handle(ValidCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<RiskAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnIdentifiedStatus()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Identified");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<RiskAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.CreateFailed");
    }
}
