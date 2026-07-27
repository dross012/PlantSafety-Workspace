// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.AccidentManagement
//  Layer:          Tests (Unit)
//  File:           CreateAccidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateAccidentCommandHandler with a mocked IAccidentRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AccidentManagement.Commands;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using AccidentAggregate = PlantSafety.Domain.AccidentManagement.Aggregates.Accident;

namespace PlantSafety.Tests.Application.AccidentManagement;

public sealed class CreateAccidentCommandHandlerTests : UnitTestBase
{
    private readonly IAccidentRepository _repository = Substitute.For<IAccidentRepository>();

    private CreateAccidentCommandHandler Sut() => new(_repository, NullLogger<CreateAccidentCommandHandler>());

    private static CreateAccidentCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow,
        ApiVersionToken: "v1",
        TenantId: TestTenantId.Value,
        SiteId: null,
        AccidentTitle: "Forklift collision, Dock 3",
        Location: "Dock 3",
        Severity: "FirstAidOnly",
        InjuredPersonCount: 1,
        OccurredAtUtc: FixedUtcNow,
        Category: "SiteSecuringAndTriage",
        OwnerId: TestUserId.Value,
        ReviewRequired: true);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAggregate()
    {
        await Sut().Handle(ValidCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<AccidentAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnReportedStatus()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Reported");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<AccidentAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.CreateFailed");
    }
}
