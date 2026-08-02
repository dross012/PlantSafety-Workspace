// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.ScoreCards.Handlers
//  Layer:          Tests (Unit)
//  File:           GetScoreCardsByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetScoreCardsByIdQueryHandler -- Phase 4 Analytics.ScoreCards
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (a ScoreCards record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.ScoreCards.Handlers;
using PlantSafety.Application.Analytics.ScoreCards.Models;
using PlantSafety.Application.Analytics.ScoreCards.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Repositories;

namespace PlantSafety.Tests.Analytics.ScoreCards.Handlers;

public sealed class GetScoreCardsByIdQueryHandlerTests : UnitTestBase
{
    private readonly IScoreCardsRepository _repo = Substitute.For<IScoreCardsRepository>();

    private GetScoreCardsByIdQueryHandler Sut() => new(_repo, NullLogger<GetScoreCardsByIdQueryHandler>());

    private static ScoreCardsItemDto ScoreCardsFor(Guid scoreCardsId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ScoreCardsId = scoreCardsId,
        TenantId = tenantId,
        Status = "Created",
        ScoreCardName = "Q3 Safety Scorecard",
        ReportingPeriod = "2026-Q3",
        KpiCategory = "Safety",
        PerformanceIndex = 87.5m,
        GeneratedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenScoreCardsExistsForTenant_ReturnsItemWithEtag()
    {
        var scoreCardsId = Guid.NewGuid();
        var scoreCards = ScoreCardsFor(scoreCardsId, TestTenantId.Value);
        _repo.GetByIdAsync(scoreCardsId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(scoreCards);

        var result = await Sut().Handle(new GetScoreCardsByIdQuery(TestTenantId.Value, scoreCardsId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ScoreCardsId.Should().Be(scoreCardsId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenScoreCardsDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ScoreCardsItemDto?)null);

        var result = await Sut().Handle(new GetScoreCardsByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.ScoreCards.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ScoreCards record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenScoreCardsBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_ScoreCardsAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a ScoreCards record that actually belongs
        // to another tenant returns no row at all -- the repository is not stubbed for that
        // (ScoreCardsId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var scoreCardsId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(scoreCardsId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ScoreCardsFor(scoreCardsId, otherTenantId));

        var result = await Sut().Handle(new GetScoreCardsByIdQuery(TestTenantId.Value, scoreCardsId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.ScoreCards.NotFound");
    }
}
