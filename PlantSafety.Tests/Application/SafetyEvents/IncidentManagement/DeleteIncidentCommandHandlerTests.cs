// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           DeleteIncidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DeleteIncidentCommandHandler (soft-delete) with a mocked
//                  IIncidentRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

namespace PlantSafety.Tests.Application.SafetyEvents.IncidentManagement;

public sealed class DeleteIncidentCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentRepository _repository = Substitute.For<IIncidentRepository>();

    private DeleteIncidentCommandHandler Sut() => new(_repository, NullLogger<DeleteIncidentCommandHandler>());

    private static DeleteIncidentCommand ValidCommand(Guid incidentId) => new()
    {
        CommandId = Guid.NewGuid(),
        CorrelationId = TestCorrelationId.Value,
        CausationId = TestCausationId.Value,
        RequestedAtUtc = FixedUtcNow,
        ApiVersionToken = "v1",
        TenantId = TestTenantId.Value,
        ActorId = TestUserId.Value,
        ETag = "\"etag-1\"",
        IncidentId = incidentId,
        DeletionReason = "Duplicate report",
    };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositorySoftDeleteAsync()
    {
        var incidentId = Guid.NewGuid();
        var cmd = ValidCommand(incidentId);

        await Sut().Handle(cmd, CancellationToken.None);

        await _repository.Received(1).SoftDeleteAsync(
            tenantId: cmd.TenantId,
            incidentId: cmd.IncidentId,
            deletionReason: cmd.DeletionReason,
            eTag: cmd.ETag!,
            actorId: cmd.ActorId,
            correlationId: cmd.CorrelationId,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        _repository.SoftDeleteAsync(
            tenantId: Arg.Any<Guid>(), incidentId: Arg.Any<Guid>(), deletionReason: Arg.Any<string>(),
            eTag: Arg.Any<string>(), actorId: Arg.Any<Guid>(), correlationId: Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("stale etag"));

        var act = async () => await Sut().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldCompleteSuccessfullyForValidCommand()
    {
        var act = async () => await Sut().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
