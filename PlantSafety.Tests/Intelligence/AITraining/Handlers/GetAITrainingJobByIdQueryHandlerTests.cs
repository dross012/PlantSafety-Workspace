// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AITraining.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAITrainingJobByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAITrainingJobByIdQueryHandler -- Phase 4
//                  Intelligence.AITraining full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation (an
//                  AITrainingJob record belonging to a different tenant must surface as NotFound,
//                  never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Handlers.GetRCAByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.AITraining.Handlers;
using PlantSafety.Application.Intelligence.AITraining.Models;
using PlantSafety.Application.Intelligence.AITraining.Queries;
using PlantSafety.Application.Intelligence.AITraining.Repositories;

namespace PlantSafety.Tests.Intelligence.AITraining.Handlers;

public sealed class GetAITrainingJobByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAITrainingJobDetailRepository _repo = Substitute.For<IAITrainingJobDetailRepository>();

    private GetAITrainingJobByIdQueryHandler Sut() => new(_repo, NullLogger<GetAITrainingJobByIdQueryHandler>());

    private static AITrainingJobDetailDto JobFor(Guid aiTrainingJobId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        AITrainingJobId = aiTrainingJobId,
        TenantId = tenantId,
        ModelName = "FatigueRiskClassifier",
        ModelVersion = "1.0.0",
        Status = "Queued",
        StartedAtUtc = FixedUtcNow,
        InitiatedBy = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAITrainingJobExistsForTenant_ReturnsItemWithEtag()
    {
        var aiTrainingJobId = Guid.NewGuid();
        var job = JobFor(aiTrainingJobId, TestTenantId.Value);
        _repo.GetByIdAsync(aiTrainingJobId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(job);

        var result = await Sut().Handle(new GetAITrainingJobByIdQuery(TestTenantId.Value, aiTrainingJobId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AITrainingJobId.Should().Be(aiTrainingJobId);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenAITrainingJobDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AITrainingJobDetailDto?)null);

        var result = await Sut().Handle(new GetAITrainingJobByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AITraining.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AITrainingJob record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAITrainingJobBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_AITrainingJob_GetById filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for an AITrainingJob record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (aiTrainingJobId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var aiTrainingJobId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(aiTrainingJobId, otherTenantId, Arg.Any<CancellationToken>()).Returns(JobFor(aiTrainingJobId, otherTenantId));

        var result = await Sut().Handle(new GetAITrainingJobByIdQuery(TestTenantId.Value, aiTrainingJobId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AITraining.NotFound");
    }
}
