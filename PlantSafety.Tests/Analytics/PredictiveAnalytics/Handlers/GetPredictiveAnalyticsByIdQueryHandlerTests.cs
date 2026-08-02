// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.PredictiveAnalytics.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPredictiveAnalyticsByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPredictiveAnalyticsByIdQueryHandler -- Phase 4
//                  Analytics.PredictiveAnalytics full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a PredictiveAnalytics record belonging to a different tenant
//                  must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Handlers;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;

namespace PlantSafety.Tests.Analytics.PredictiveAnalytics.Handlers;

public sealed class GetPredictiveAnalyticsByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPredictiveAnalyticsRepository _repo = Substitute.For<IPredictiveAnalyticsRepository>();

    private GetPredictiveAnalyticsByIdQueryHandler Sut() => new(_repo, NullLogger<GetPredictiveAnalyticsByIdQueryHandler>());

    private static PredictiveAnalyticsItemDto PredictionFor(Guid predictionId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        PredictionId = predictionId,
        TenantId = tenantId,
        ModelName = "IncidentRiskModel-v3",
        Score = 0.62m,
        Status = "Created",
        GeneratedAtUtc = FixedUtcNow,
        GeneratedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenPredictionExistsForTenant_ReturnsItemWithEtag()
    {
        var predictionId = Guid.NewGuid();
        var prediction = PredictionFor(predictionId, TestTenantId.Value);
        _repo.GetByIdAsync(predictionId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(prediction);

        var result = await Sut().Handle(new GetPredictiveAnalyticsByIdQuery(TestTenantId.Value, predictionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PredictionId.Should().Be(predictionId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenPredictionDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PredictiveAnalyticsItemDto?)null);

        var result = await Sut().Handle(new GetPredictiveAnalyticsByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.PredictiveAnalytics.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing PredictiveAnalytics record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenPredictionBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_PredictiveAnalytics_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (predictionId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var predictionId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(predictionId, otherTenantId, Arg.Any<CancellationToken>()).Returns(PredictionFor(predictionId, otherTenantId));

        var result = await Sut().Handle(new GetPredictiveAnalyticsByIdQuery(TestTenantId.Value, predictionId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.PredictiveAnalytics.NotFound");
    }
}
