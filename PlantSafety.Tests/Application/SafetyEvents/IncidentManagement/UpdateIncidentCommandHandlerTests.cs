// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           UpdateIncidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for UpdateIncidentCommandHandler with a mocked IIncidentRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

namespace PlantSafety.Tests.Application.SafetyEvents.IncidentManagement;

public sealed class UpdateIncidentCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentRepository _repository = Substitute.For<IIncidentRepository>();

    private UpdateIncidentCommandHandler Sut() => new(_repository, NullLogger<UpdateIncidentCommandHandler>());

    private static UpdateIncidentCommand ValidCommand(Guid incidentId) => new()
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
        Title = "Updated title",
        Description = "Updated description",
        Severity = "Serious",
        Location = "Lab 3",
        AssignedInvestigatorId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryUpdateAsync()
    {
        var incidentId = Guid.NewGuid();
        var cmd = ValidCommand(incidentId);

        await Sut().Handle(cmd, CancellationToken.None);

        await _repository.Received(1).UpdateAsync(
            tenantId: cmd.TenantId,
            incidentId: cmd.IncidentId,
            title: cmd.Title,
            description: cmd.Description,
            severity: cmd.Severity,
            location: cmd.Location,
            assignedInvestigatorId: cmd.AssignedInvestigatorId,
            eTag: cmd.ETag!,
            actorId: cmd.ActorId,
            correlationId: cmd.CorrelationId,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSameIncidentId()
    {
        var incidentId = Guid.NewGuid();
        var result = await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);
        result.Should().Be(incidentId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsConcurrencyConflict_ShouldPropagate()
    {
        _repository.UpdateAsync(
            tenantId: Arg.Any<Guid>(), incidentId: Arg.Any<Guid>(), title: Arg.Any<string?>(), description: Arg.Any<string?>(),
            severity: Arg.Any<string?>(), location: Arg.Any<string?>(), assignedInvestigatorId: Arg.Any<Guid?>(),
            eTag: Arg.Any<string>(), actorId: Arg.Any<Guid>(), correlationId: Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("stale etag"));

        var act = async () => await Sut().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
