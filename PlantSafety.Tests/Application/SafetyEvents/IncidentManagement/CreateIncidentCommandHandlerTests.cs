// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           CreateIncidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Updated:        2026-07-27 -- Rewritten for the aggregate-wired CreateIncidentCommandHandler
//                  (IncidentManagement.Create -> IIncidentRepository.CreateAsync(aggregate, ...)).
//                  Also asserts that Create's domain invariants (enforced by the aggregate factory,
//                  not just Application-layer FluentValidation) are actually reachable from the
//                  handler, and that the aggregate's own generated identity is what is persisted
//                  and returned -- not a handler-local id.
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.2
//  Description:    Unit tests for CreateIncidentCommandHandler with a mocked IIncidentRepository
//                  and IDateTimeProvider. Also asserts that the
//                  IncidentMetrics.IncidentsCreated counter (added in a prior pass) is incremented
//                  exactly once per successful creation, using System.Diagnostics.Metrics.MeterListener
//                  since Counter<T> exposes no readable state directly.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Diagnostics.Metrics;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Tests.Application.SafetyEvents.IncidentManagement;

public sealed class CreateIncidentCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentRepository _repository = Substitute.For<IIncidentRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();

    public CreateIncidentCommandHandlerTests()
    {
        _clock.UtcNowOffset.Returns(new DateTimeOffset(FixedUtcNow));
        _clock.UtcNow.Returns(FixedUtcNow);
    }

    private CreateIncidentCommandHandler Sut() => new(_repository, _clock, NullLogger<CreateIncidentCommandHandler>());

    private static CreateIncidentCommand ValidCommand() => new()
    {
        CommandId = Guid.NewGuid(),
        CorrelationId = TestCorrelationId.Value,
        CausationId = TestCausationId.Value,
        RequestedAtUtc = FixedUtcNow,
        ApiVersionToken = "v1",
        TenantId = TestTenantId.Value,
        ActorId = TestUserId.Value,
        SiteId = Guid.NewGuid(),
        Title = "Chemical spill",
        Description = "Small chemical spill in Lab 2",
        IncidentOccurredAtUtc = new DateTimeOffset(FixedUtcNow),
        Severity = "Moderate",
        Location = "Lab 2",
        ReportedByWorkerId = Guid.NewGuid(),
        AssignedInvestigatorId = null,
    };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryCreateAsyncWithAggregate()
    {
        await Sut().Handle(ValidCommand(), CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            aggregate: Arg.Any<IncidentAggregate>(),
            actorId: Arg.Any<Guid>(),
            correlationId: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnNonEmptyIncidentId()
    {
        var incidentId = await Sut().Handle(ValidCommand(), CancellationToken.None);
        incidentId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPassAggregateWithTenantSiteAndTitleToRepository()
    {
        var cmd = ValidCommand();
        IncidentAggregate? capturedAggregate = null;

        await _repository.CreateAsync(
            aggregate: Arg.Do<IncidentAggregate>(a => capturedAggregate = a),
            actorId: Arg.Any<Guid>(),
            correlationId: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>());

        await Sut().Handle(cmd, CancellationToken.None);

        capturedAggregate.Should().NotBeNull();
        capturedAggregate!.TenantId.Should().Be(cmd.TenantId);
        capturedAggregate.SiteId.Should().Be(cmd.SiteId);
        capturedAggregate.Title.Should().Be(cmd.Title);
        capturedAggregate.Description.Should().Be(cmd.Description);
        capturedAggregate.Severity.Should().Be(cmd.Severity);
        capturedAggregate.Location.Should().Be(cmd.Location);
        capturedAggregate.ReportedByWorkerId.Value.Should().Be(cmd.ReportedByWorkerId);
    }

    [Fact]
    public async Task Handle_ReturnedIncidentId_ShouldMatchAggregateIdentity()
    {
        var cmd = ValidCommand();
        IncidentAggregate? capturedAggregate = null;

        await _repository.CreateAsync(
            aggregate: Arg.Do<IncidentAggregate>(a => capturedAggregate = a),
            actorId: Arg.Any<Guid>(),
            correlationId: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>());

        var incidentId = await Sut().Handle(cmd, CancellationToken.None);

        capturedAggregate.Should().NotBeNull();
        incidentId.Should().Be(capturedAggregate!.IncidentManagementId.Value);
    }

    [Fact]
    public async Task Handle_WithWhitespaceOnlyTitle_ShouldThrowArgumentException_BeforeCallingRepository()
    {
        // The Domain aggregate's Create factory rejects a whitespace-only Title even though
        // FluentValidation's NotEmpty already blocks it upstream -- this asserts the invariant is
        // now *also* enforced at the aggregate level the handler goes through, not merely at the
        // Application-layer validator, and that the repository is never called when it fails.
        var cmd = ValidCommand() with { Title = "   " };

        var act = async () => await Sut().Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        await _repository.DidNotReceive().CreateAsync(
            aggregate: Arg.Any<IncidentAggregate>(),
            actorId: Arg.Any<Guid>(),
            correlationId: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldIncrementIncidentsCreatedCounter()
    {
        long observedTotal = 0;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == "PlantSafety.Application.Incidents" && instrument.Name == "plantsafety.incidents.created.total")
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) => observedTotal += measurement);
        listener.Start();

        await Sut().Handle(ValidCommand(), CancellationToken.None);

        listener.Dispose();
        observedTotal.Should().Be(1);
    }
}
