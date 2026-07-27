// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           CreateIncidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
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
    public async Task Handle_WithValidCommand_ShouldCallRepositoryCreateAsync()
    {
        await Sut().Handle(ValidCommand(), CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            incidentId: Arg.Any<Guid>(),
            tenantId: Arg.Any<Guid>(),
            siteId: Arg.Any<Guid>(),
            title: Arg.Any<string>(),
            description: Arg.Any<string>(),
            incidentOccurredAtUtc: Arg.Any<DateTimeOffset>(),
            severity: Arg.Any<string>(),
            location: Arg.Any<string>(),
            reportedByWorkerId: Arg.Any<Guid>(),
            assignedInvestigatorId: Arg.Any<Guid?>(),
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
    public async Task Handle_WithValidCommand_ShouldPassTenantAndSiteToRepository()
    {
        var cmd = ValidCommand();
        await Sut().Handle(cmd, CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            incidentId: Arg.Any<Guid>(),
            tenantId: cmd.TenantId,
            siteId: cmd.SiteId,
            title: cmd.Title,
            description: cmd.Description,
            incidentOccurredAtUtc: cmd.IncidentOccurredAtUtc,
            severity: cmd.Severity,
            location: cmd.Location,
            reportedByWorkerId: cmd.ReportedByWorkerId,
            assignedInvestigatorId: cmd.AssignedInvestigatorId,
            actorId: cmd.ActorId,
            correlationId: cmd.CorrelationId,
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
