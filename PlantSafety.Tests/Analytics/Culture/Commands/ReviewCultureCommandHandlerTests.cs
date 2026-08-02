// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Commands
//  Layer:          Tests (Unit)
//  File:           ReviewCultureCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Analytics.Culture full build-out.
//                  ReviewCultureCommandHandler must forward the caller's ETag to
//                  ICultureRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Interfaces.Persistence;
using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Tests.Analytics.Culture.Commands;

public sealed class ReviewCultureCommandHandlerTests : UnitTestBase
{
    private readonly ICultureRepository _repo = Substitute.For<ICultureRepository>();

    private ReviewCultureCommandHandler Sut() => new(_repo, NullLogger<ReviewCultureCommandHandler>());

    private static CultureAggregate CreatedCulture() =>
        CultureAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            surveyPeriod: "2026-Q3",
            cultureIndexScore: 72.5m,
            engagementScore: 68.0m,
            participationRate: 81.0m,
            topRiskTheme: "Fatigue");

    private static ReviewCultureCommand ReviewCommandFor(CultureAggregate culture, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            CultureId: culture.CultureId.Value,
            ReviewedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var culture = CreatedCulture();
        _repo.GetByIdAsync(Arg.Any<CultureId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(culture);

        var command = ReviewCommandFor(culture, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(culture, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var culture = CreatedCulture();
        _repo.GetByIdAsync(Arg.Any<CultureId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(culture);
        _repo.UpdateAsync(Arg.Any<CultureAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Culture was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ReviewCommandFor(culture, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
