// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PlanningProjects.ProjectAggregate.Handlers
//  Layer:          Tests (Unit)
//  File:           GetProjectByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetProjectByIdQueryHandler -- Phase 4 PlanningProjects.Project
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (a Project record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandlerTests. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Handlers;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Queries;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Repositories;

namespace PlantSafety.Tests.PlanningProjects.ProjectAggregate.Handlers;

public sealed class GetProjectByIdQueryHandlerTests : UnitTestBase
{
    private readonly IProjectReadRepository _repo = Substitute.For<IProjectReadRepository>();

    private GetProjectByIdQueryHandler Sut() => new(_repo, NullLogger<GetProjectByIdQueryHandler>());

    private static ProjectDetailDto ProjectFor(Guid projectId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ProjectId = projectId,
        TenantId = tenantId,
        Name = "New Plant Expansion",
        Status = "Planning",
        TargetCompletionUtc = FixedUtcNow,
        OwnerId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenProjectExistsForTenant_ReturnsItemWithEtag()
    {
        var projectId = Guid.NewGuid();
        var project = ProjectFor(projectId, TestTenantId.Value);
        _repo.GetByIdAsync(projectId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(project);

        var result = await Sut().Handle(new GetProjectByIdQuery(TestTenantId.Value, projectId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProjectId.Should().Be(projectId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProjectDetailDto?)null);

        var result = await Sut().Handle(new GetProjectByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PlanningProjects.Project.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Project record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenProjectBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_PlanningProject_GetById filters by TenantId in SQL, so
        // a lookup scoped to TestTenantId for a Project record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (ProjectId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var projectId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(projectId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ProjectFor(projectId, otherTenantId));

        var result = await Sut().Handle(new GetProjectByIdQuery(TestTenantId.Value, projectId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PlanningProjects.Project.NotFound");
    }
}
