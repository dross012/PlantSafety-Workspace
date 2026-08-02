// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Dashboard.Commands
//  Layer:          Tests (Unit)
//  File:           PublishDashboardCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Analytics.Dashboard full build-out.
//                  PublishDashboardCommandHandler must forward the caller's ETag to
//                  IDashboardRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.Dashboard.Commands;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Domain.Analytics.Dashboard.ValueObjects.Ids;
using DashboardAggregate = PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard;

namespace PlantSafety.Tests.Analytics.Dashboard.Commands;

public sealed class PublishDashboardCommandHandlerTests : UnitTestBase
{
    private readonly IDashboardRepository _repo = Substitute.For<IDashboardRepository>();

    private PublishDashboardCommandHandler Sut() => new(_repo, NullLogger<PublishDashboardCommandHandler>());

    private static DashboardAggregate CreatedDashboard() =>
        DashboardAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            dashboardName: "Plant Safety Overview",
            dashboardType: "Executive",
            ownerId: TestUserId,
            refreshIntervalSeconds: 60,
            widgetCount: 6);

    private static PublishDashboardCommand PublishCommandFor(DashboardAggregate dashboard, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            DashboardId: dashboard.DashboardId.Value,
            PublishedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var dashboard = CreatedDashboard();
        _repo.GetByIdAsync(Arg.Any<DashboardId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(dashboard);

        var command = PublishCommandFor(dashboard, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(dashboard, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var dashboard = CreatedDashboard();
        _repo.GetByIdAsync(Arg.Any<DashboardId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(dashboard);
        _repo.UpdateAsync(Arg.Any<DashboardAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Dashboard was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = PublishCommandFor(dashboard, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
