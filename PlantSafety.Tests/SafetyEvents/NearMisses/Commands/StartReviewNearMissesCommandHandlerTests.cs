// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.NearMisses.Commands
//  Layer:          Tests (Unit)
//  File:           StartReviewNearMissesCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.NearMisses full build-out.
//                  StartReviewNearMissesCommandHandler must forward the caller's ETag to
//                  INearMissesRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.NearMisses.Commands;
using PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces;
using PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;
using NearMissesAggregate = PlantSafety.Domain.SafetyEvents.NearMisses.Aggregates.NearMisses;

namespace PlantSafety.Tests.SafetyEvents.NearMisses.Commands;

public sealed class StartReviewNearMissesCommandHandlerTests : UnitTestBase
{
    private readonly INearMissesRepository _repo = Substitute.For<INearMissesRepository>();

    private StartReviewNearMissesCommandHandler Sut() => new(_repo, NullLogger<StartReviewNearMissesCommandHandler>());

    private static NearMissesAggregate ReportedNearMisses() =>
        NearMissesAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            category: "SlipTripFall",
            potentialSeverity: "Medium",
            reportedAtUtc: FixedTimestamp);

    private static StartReviewNearMissesCommand StartReviewCommandFor(NearMissesAggregate nearMisses, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            NearMissesId: nearMisses.NearMissesId.Value,
            ReviewedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var nearMisses = ReportedNearMisses();
        _repo.GetByIdAsync(Arg.Any<NearMissesId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(nearMisses);

        var command = StartReviewCommandFor(nearMisses, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(nearMisses, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var nearMisses = ReportedNearMisses();
        _repo.GetByIdAsync(Arg.Any<NearMissesId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(nearMisses);
        _repo.UpdateAsync(Arg.Any<NearMissesAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("NearMisses was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartReviewCommandFor(nearMisses, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNearMissesDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<NearMissesId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NearMissesAggregate?)null);

        var command = StartReviewCommandFor(ReportedNearMisses(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.NearMisses.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing NearMisses record must map to HTTP 404, not the default 500");
    }
}
