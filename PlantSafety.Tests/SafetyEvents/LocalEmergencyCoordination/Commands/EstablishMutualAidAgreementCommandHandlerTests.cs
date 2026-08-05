// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Commands
//  Layer:          Tests (Unit)
//  File:           EstablishMutualAidAgreementCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for EstablishMutualAidAgreementCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Commands;

public sealed class EstablishMutualAidAgreementCommandHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private EstablishMutualAidAgreementCommandHandler Sut() => new(_repo, NullLogger<EstablishMutualAidAgreementCommandHandler>());

    private static EstablishMutualAidAgreementCommand CommandFor(LocalEmergencyCoordinationAggregate agg, string? eTag = null) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        LocalEmergencyCoordinationId: agg.LocalEmergencyCoordinationId.Value,
        AgencyName: "Metro Fire Dept",
        Discipline: "Fire",
        DocumentVersion: 1,
        EffectiveFromUtc: FixedUtcNow,
        RenewalDueAtUtc: FixedUtcNow.AddYears(1),
        EstablishedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_EstablishesAgreementAndForwardsCallerEtag()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);

        var result = await Sut().Handle(CommandFor(agg, "0x0000000000001234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RenewalStatus.Should().Be("Current");
        await _repo.Received(1).UpdateAsync(agg, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCaseDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LocalEmergencyCoordinationAggregate?)null);

        var result = await Sut().Handle(CommandFor(LocalEmergencyCoordinationTestSupport.NewCase()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);
        _repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(agg, "stale"), CancellationToken.None));
    }
}
