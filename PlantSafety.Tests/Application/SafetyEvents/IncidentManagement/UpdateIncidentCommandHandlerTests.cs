// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           UpdateIncidentCommandHandlerTests.cs
//  Created:        2026-07-27
//  Updated:        2026-07-27 -- Rewritten for the aggregate-wired UpdateIncidentCommandHandler
//                  (read-before-write via IIncidentReadRepository -> IncidentManagement.Rehydrate
//                  -> UpdateDetails -> IIncidentRepository.UpdateAsync(aggregate, diff, ...)).
//                  Adds handler-level coverage the pre-existing
//                  IncidentManagementAggregateTests.cs domain tests do not: that the handler
//                  itself now rejects an update against an Archived incident
//                  (IncidentManagementInvalidStatusTransitionException, never reaching
//                  IIncidentRepository.UpdateAsync -- the old direct-SQL handler had no such
//                  guard and would have sent the update straight to SQL trusting the WHERE
//                  IsDeleted = 0 filter alone) and that a missing incident now surfaces
//                  IncidentManagementNotFoundException instead of the previous, less accurate
//                  ConcurrencyConflictException.
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.2
//  Description:    Unit tests for UpdateIncidentCommandHandler with mocked IIncidentRepository and
//                  IIncidentReadRepository. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Tests.Application.SafetyEvents.IncidentManagement;

public sealed class UpdateIncidentCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentRepository _repository = Substitute.For<IIncidentRepository>();
    private readonly IIncidentReadRepository _readRepository = Substitute.For<IIncidentReadRepository>();

    private UpdateIncidentCommandHandler Sut() => new(_repository, _readRepository, NullLogger<UpdateIncidentCommandHandler>());

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

    private static IncidentReadModel ExistingRow(Guid incidentId, string status = "Reported") => new()
    {
        IncidentId = incidentId,
        TenantId = TestTenantId.Value,
        SiteId = Guid.NewGuid(),
        Title = "Chemical spill",
        Description = "Small chemical spill in Lab 2",
        IncidentOccurredAtUtc = new DateTimeOffset(FixedUtcNow),
        ReportedAtUtc = new DateTimeOffset(FixedUtcNow),
        Severity = "Moderate",
        Status = status,
        Location = "Lab 2",
        ReportedByWorkerId = TestUserId.Value,
        AssignedInvestigatorId = null,
        ClosedAtUtc = null,
        LastModifiedByActorId = TestUserId.Value,
        LastModifiedAtUtc = new DateTimeOffset(FixedUtcNow),
        ETag = "\"etag-1\"",
        CreatedAtUtc = new DateTimeOffset(FixedUtcNow),
        CreatedBy = TestUserId.Value,
    };

    private void ArrangeExistingRow(Guid incidentId, string status = "Reported") =>
        _readRepository.GetByIdAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>())
            .Returns(ExistingRow(incidentId, status));

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryUpdateAsyncWithDiffFields()
    {
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId);
        var cmd = ValidCommand(incidentId);

        await Sut().Handle(cmd, CancellationToken.None);

        await _repository.Received(1).UpdateAsync(
            aggregate: Arg.Any<IncidentAggregate>(),
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
    public async Task Handle_ShouldFirstLoadCurrentRowFromReadRepository()
    {
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId);

        await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);

        await _readRepository.Received(1).GetByIdAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSameIncidentId()
    {
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId);

        var result = await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);
        result.Should().Be(incidentId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsConcurrencyConflict_ShouldPropagate()
    {
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId);

        _repository.UpdateAsync(
            aggregate: Arg.Any<IncidentAggregate>(), title: Arg.Any<string?>(), description: Arg.Any<string?>(),
            severity: Arg.Any<string?>(), location: Arg.Any<string?>(), assignedInvestigatorId: Arg.Any<Guid?>(),
            eTag: Arg.Any<string>(), actorId: Arg.Any<Guid>(), correlationId: Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale etag", TestCorrelationId.Value));

        var act = async () => await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    // ---- Handler-level invariant enforcement (new behaviour) ---------------------

    [Fact]
    public async Task Handle_WhenIncidentDoesNotExist_ShouldThrowNotFound_AndNeverCallUpdateAsync()
    {
        // GetByIdAsync returns null (not found / already soft-deleted / wrong tenant): the old
        // direct-SQL handler would have sent the update straight to
        // IIncidentRepository.UpdateAsync and only discovered the problem when the stored
        // procedure's WHERE clause matched zero rows, surfacing the generic
        // ConcurrencyConflictException. The aggregate-wired handler now fails fast with a
        // domain-specific NotFound exception and never calls UpdateAsync at all.
        var incidentId = Guid.NewGuid();
        _readRepository.GetByIdAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>())
            .Returns((IncidentReadModel?)null);

        var act = async () => await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);

        await act.Should().ThrowAsync<IncidentManagementNotFoundException>();
        await _repository.DidNotReceive().UpdateAsync(
            aggregate: Arg.Any<IncidentAggregate>(), title: Arg.Any<string?>(), description: Arg.Any<string?>(),
            severity: Arg.Any<string?>(), location: Arg.Any<string?>(), assignedInvestigatorId: Arg.Any<Guid?>(),
            eTag: Arg.Any<string>(), actorId: Arg.Any<Guid>(), correlationId: Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenIncidentIsArchived_ShouldThrowInvalidStatusTransition_AndNeverCallUpdateAsync()
    {
        // The rehydrated aggregate enforces its own lifecycle invariant (no updates once
        // Archived) independently of the SQL layer. The old direct-SQL handler had no
        // in-process concept of "Archived" at all -- it relied entirely on the stored
        // procedure's WHERE IsDeleted = 0 filter and would have silently produced a
        // ConcurrencyConflictException with no domain-specific signal that the real problem was
        // an illegal lifecycle transition, not a stale ETag.
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId, status: "Archived");

        var act = async () => await Sut().Handle(ValidCommand(incidentId), CancellationToken.None);

        await act.Should().ThrowAsync<IncidentManagementInvalidStatusTransitionException>();
        await _repository.DidNotReceive().UpdateAsync(
            aggregate: Arg.Any<IncidentAggregate>(), title: Arg.Any<string?>(), description: Arg.Any<string?>(),
            severity: Arg.Any<string?>(), location: Arg.Any<string?>(), assignedInvestigatorId: Arg.Any<Guid?>(),
            eTag: Arg.Any<string>(), actorId: Arg.Any<Guid>(), correlationId: Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOnlyTitleProvided_ShouldPassNullForOtherFields_PreservingPartialUpdateSemantics()
    {
        // Asserts the ISNULL/COALESCE partial-update SQL contract is preserved: only the fields
        // actually present on the command are forwarded non-null to the repository, even though
        // the rehydrated aggregate now holds a fully-merged in-memory state after UpdateDetails.
        var incidentId = Guid.NewGuid();
        ArrangeExistingRow(incidentId);
        var cmd = ValidCommand(incidentId) with
        {
            Title = "Only title changed",
            Description = null,
            Severity = null,
            Location = null,
            AssignedInvestigatorId = null,
        };

        await Sut().Handle(cmd, CancellationToken.None);

        await _repository.Received(1).UpdateAsync(
            aggregate: Arg.Any<IncidentAggregate>(),
            title: "Only title changed",
            description: null,
            severity: null,
            location: null,
            assignedInvestigatorId: null,
            eTag: cmd.ETag!,
            actorId: cmd.ActorId,
            correlationId: cmd.CorrelationId,
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
