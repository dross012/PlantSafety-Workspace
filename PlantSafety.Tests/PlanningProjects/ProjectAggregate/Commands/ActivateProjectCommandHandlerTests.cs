// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PlanningProjects.ProjectAggregate.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateProjectCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 PlanningProjects.Project full build-out.
//                  ActivateProjectCommandHandler must forward the caller's ETag to
//                  IProjectRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Commands
//                  .ApproveRootCauseAnalysisCommandHandlerTests. Isolated, deterministic, <50ms
//                  per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;
using PlantSafety.Domain.PlanningProjects.Interfaces;
using PlantSafety.Domain.PlanningProjects.ValueObjects;
using ProjectAgg = PlantSafety.Domain.PlanningProjects.Aggregates.Project;

namespace PlantSafety.Tests.PlanningProjects.ProjectAggregate.Commands;

public sealed class ActivateProjectCommandHandlerTests : UnitTestBase
{
    private readonly IProjectRepository _repo = Substitute.For<IProjectRepository>();

    private ActivateProjectCommandHandler Sut() => new(_repo, NullLogger<ActivateProjectCommandHandler>());

    private static ProjectAgg PlanningProject() =>
        ProjectAgg.Create(
            tenantId: TestTenantId.Value,
            siteId: null,
            name: "New Plant Expansion",
            targetCompletionUtc: FixedTimestamp,
            ownerId: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    private static ActivateProjectCommand ActivateCommandFor(ProjectAgg project, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ProjectId: project.ProjectId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var project = PlanningProject();
        _repo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(project);

        var command = ActivateCommandFor(project, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(project, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var project = PlanningProject();
        _repo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(project);
        _repo.UpdateAsync(Arg.Any<ProjectAgg>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Project was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(project, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProjectAgg?)null);

        var command = ActivateCommandFor(PlanningProject(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PlanningProjects.Project.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Project record must map to HTTP 404, not the default 500");
    }
}
