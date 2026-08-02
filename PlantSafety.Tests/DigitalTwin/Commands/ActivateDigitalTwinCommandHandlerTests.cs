// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateDigitalTwinCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 DigitalTwin full build-out.
//                  ActivateDigitalTwinCommandHandler must forward the caller's ETag to
//                  IDigitalTwinRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.DigitalTwin.Commands;
using PlantSafety.Domain.DigitalTwin.Repositories;
using PlantSafety.Domain.DigitalTwin.ValueObjects;
using DigitalTwinAggregate = PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin;

namespace PlantSafety.Tests.DigitalTwin.Commands;

public sealed class ActivateDigitalTwinCommandHandlerTests : UnitTestBase
{
    private readonly IDigitalTwinRepository _repo = Substitute.For<IDigitalTwinRepository>();

    private ActivateDigitalTwinCommandHandler Sut() => new(_repo, NullLogger<ActivateDigitalTwinCommandHandler>());

    private static DigitalTwinAggregate CreatedDigitalTwin() =>
        DigitalTwinAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            twinName: "Line 3 Conveyor Twin",
            modelType: "PhysicsBasedSimulation");

    private static ActivateDigitalTwinCommand ActivateCommandFor(DigitalTwinAggregate aggregate, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            DigitalTwinId: aggregate.DigitalTwinId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var aggregate = CreatedDigitalTwin();
        _repo.GetByIdAsync(Arg.Any<DigitalTwinId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);

        var command = ActivateCommandFor(aggregate, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(aggregate, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var aggregate = CreatedDigitalTwin();
        _repo.GetByIdAsync(Arg.Any<DigitalTwinId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);
        _repo.UpdateAsync(Arg.Any<DigitalTwinAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("DigitalTwin was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(aggregate, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenDigitalTwinDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<DigitalTwinId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DigitalTwinAggregate?)null);

        var command = ActivateCommandFor(CreatedDigitalTwin(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing DigitalTwin record must map to HTTP 404, not the default 500");
    }
}
