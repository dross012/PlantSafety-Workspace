// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.GlobalSafetyInsights.Coordination
//  Layer:          Tests (Unit)
//  File:           CrossSiteIncidentEscalationCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CrossSiteIncidentEscalationCommandHandler: escalates the most
//                  recent open Incident at a site when one exists, creates a new cross-site-flagged
//                  Incident when none exists, and reports a per-site failure without aborting the
//                  whole batch. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using MediatR;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Escalate;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Handlers;

namespace PlantSafety.Tests.Application.GlobalSafetyInsights.Coordination;

/// <summary>Unit tests for <see cref="CrossSiteIncidentEscalationCommandHandler"/>.</summary>
public sealed class CrossSiteIncidentEscalationCommandHandlerTests : UnitTestBase
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly IIncidentReadRepository _incidentReadRepository = Substitute.For<IIncidentReadRepository>();

    private CrossSiteIncidentEscalationCommandHandler Sut()
        => new(_sender, _incidentReadRepository, NullLogger<CrossSiteIncidentEscalationCommandHandler>());

    private static IncidentReadModel OpenIncident(Guid siteId) => new()
    {
        IncidentId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        SiteId = siteId,
        Title = "Existing",
        Description = "Existing",
        IncidentOccurredAtUtc = DateTimeOffset.UtcNow,
        ReportedAtUtc = DateTimeOffset.UtcNow,
        Severity = "Moderate",
        Status = "Reported",
        Location = "Floor 1",
        ReportedByWorkerId = Guid.NewGuid(),
        CreatedAtUtc = DateTimeOffset.UtcNow,
        CreatedBy = Guid.NewGuid(),
        LastModifiedAtUtc = DateTimeOffset.UtcNow,
        LastModifiedByActorId = Guid.NewGuid(),
        ETag = "1",
    };

    [Fact]
    public async Task Handle_WithOpenIncidentAtSite_EscalatesExistingIncident()
    {
        var site = Guid.NewGuid();
        var existing = OpenIncident(site);

        _incidentReadRepository.ListAsync(
                TestTenantId.Value, site, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<string>(), Arg.Any<string>(), 1, 25, Arg.Any<CancellationToken>())
            .Returns(([existing], 1));

        _sender.Send(Arg.Any<EscalateIncidentCommand>(), Arg.Any<CancellationToken>())
            .Returns(existing.IncidentId);

        var command = new CrossSiteIncidentEscalationCommand(TestTenantId.Value, [site], "Cross-site spike", null, TestCorrelationId.Value, TestCausationId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcomes.Should().ContainSingle(o => o.SiteId == site && o.Success && o.Action == "Escalated" && o.IncidentId == existing.IncidentId);
        await _sender.Received(1).Send(Arg.Is<EscalateIncidentCommand>(c => c.IncidentId == existing.IncidentId), Arg.Any<CancellationToken>());
        await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateIncidentCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoOpenIncidentAtSite_CreatesNewCrossSiteFlaggedIncident()
    {
        var site = Guid.NewGuid();

        _incidentReadRepository.ListAsync(
                TestTenantId.Value, site, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<string>(), Arg.Any<string>(), 1, 25, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<IncidentReadModel>(), 0));

        var newIncidentId = Guid.NewGuid();
        _sender.Send(Arg.Any<CreateIncidentCommand>(), Arg.Any<CancellationToken>())
            .Returns(newIncidentId);

        var command = new CrossSiteIncidentEscalationCommand(TestTenantId.Value, [site], "Cross-site spike", null, TestCorrelationId.Value, TestCausationId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcomes.Should().ContainSingle(o => o.SiteId == site && o.Success && o.Action == "Created" && o.IncidentId == newIncidentId);
        await _sender.Received(1).Send(Arg.Is<CreateIncidentCommand>(c => c.SiteId == site && c.Severity == "Critical"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDispatchThrows_RecordsFailureOutcomeWithoutAbortingOtherSites()
    {
        var failingSite = Guid.NewGuid();
        var succeedingSite = Guid.NewGuid();

        _incidentReadRepository.ListAsync(
                TestTenantId.Value, failingSite, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<string>(), Arg.Any<string>(), 1, 25, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(IReadOnlyList<IncidentReadModel>, int)>(new InvalidOperationException("boom")));

        _incidentReadRepository.ListAsync(
                TestTenantId.Value, succeedingSite, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<string>(), Arg.Any<string>(), 1, 25, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<IncidentReadModel>(), 0));

        var createdId = Guid.NewGuid();
        _sender.Send(Arg.Any<CreateIncidentCommand>(), Arg.Any<CancellationToken>()).Returns(createdId);

        var command = new CrossSiteIncidentEscalationCommand(
            TestTenantId.Value, [failingSite, succeedingSite], "Cross-site spike", null, TestCorrelationId.Value, TestCausationId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AllSucceeded.Should().BeFalse();
        result.Value.Outcomes.Should().Contain(o => o.SiteId == failingSite && !o.Success);
        result.Value.Outcomes.Should().Contain(o => o.SiteId == succeedingSite && o.Success);
    }
}
