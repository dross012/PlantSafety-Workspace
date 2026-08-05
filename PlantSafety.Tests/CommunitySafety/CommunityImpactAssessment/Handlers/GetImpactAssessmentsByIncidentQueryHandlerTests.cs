// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityImpactAssessment.Handlers
//  Layer:          Tests (Unit)
//  File:           GetImpactAssessmentsByIncidentQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetImpactAssessmentsByIncidentQueryHandler. Covers successful
//                  retrieval, empty-result handling, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Models;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityImpactAssessment.Handlers;

public sealed class GetImpactAssessmentsByIncidentQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityImpactAssessmentRepository _repo = Substitute.For<ICommunityImpactAssessmentRepository>();

    private GetImpactAssessmentsByIncidentQueryHandler Sut() => new(_repo, NullLogger<GetImpactAssessmentsByIncidentQueryHandler>());

    [Fact]
    public async Task Handle_WithMatchingAssessments_ReturnsSuccessWithItems()
    {
        var incidentId = Guid.NewGuid();
        var item = new CommunityImpactAssessmentItemDto
        {
            CommunityImpactAssessmentId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            SiteId = Guid.NewGuid(),
            IncidentId = incidentId,
            Severity = "Severe",
            AffectedArea = "North Ridge",
            Description = "desc",
            RequiresRightToKnowReporting = true,
            AssessmentStatus = "Recorded",
            AssessedAtUtc = FixedUtcNow,
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };
        _repo.GetImpactAssessmentItemsByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>())
            .Returns(new List<CommunityImpactAssessmentItemDto> { item });

        var result = await Sut().Handle(new GetImpactAssessmentsByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle().Which.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public async Task Handle_WithNoMatchingAssessments_ReturnsSuccessWithEmptyItems()
    {
        _repo.GetImpactAssessmentItemsByIncidentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityImpactAssessmentItemDto>());

        var result = await Sut().Handle(new GetImpactAssessmentsByIncidentQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetImpactAssessmentItemsByIncidentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunityImpactAssessmentItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetImpactAssessmentsByIncidentQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityImpactAssessment.RetrievalFailed");
    }
}
