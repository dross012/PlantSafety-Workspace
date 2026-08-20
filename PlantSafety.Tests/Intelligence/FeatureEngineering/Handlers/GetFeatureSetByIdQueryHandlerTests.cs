// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.FeatureEngineering.Handlers
//  Layer:          Tests (Unit)
//  File:           GetFeatureSetByIdQueryHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetFeatureSetByIdQueryHandler. Covers successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and tenant isolation
//                  (a record belonging to a different tenant must surface as NotFound, never
//                  leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Handlers.GetDataLakeActivityByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.FeatureEngineering.Handlers;
using PlantSafety.Application.Intelligence.FeatureEngineering.Models;
using PlantSafety.Application.Intelligence.FeatureEngineering.Queries;
using PlantSafety.Application.Intelligence.FeatureEngineering.Repositories;

namespace PlantSafety.Tests.Intelligence.FeatureEngineering.Handlers;

public sealed class GetFeatureSetByIdQueryHandlerTests : UnitTestBase
{
    private readonly IFeatureSetDetailRepository _repo = Substitute.For<IFeatureSetDetailRepository>();

    private GetFeatureSetByIdQueryHandler Sut() => new(_repo, NullLogger<GetFeatureSetByIdQueryHandler>());

    private static FeatureSetDetailDto FeatureSetFor(Guid featureSetId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        FeatureSetId = featureSetId,
        TenantId = tenantId,
        Name = "IncidentRiskFeatures",
        Version = "1.0",
        DataSourcePath = "masked://datalake/incident-risk",
        IsPublished = false,
        FeatureNamesCsv = "worker_fatigue_score,anomaly_score",
        FeatureCount = 2,
        ComputedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenFeatureSetExistsForTenant_ReturnsItemWithEtag()
    {
        var featureSetId = Guid.NewGuid();
        var featureSet = FeatureSetFor(featureSetId, TestTenantId.Value);
        _repo.GetByIdAsync(featureSetId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(featureSet);

        var result = await Sut().Handle(new GetFeatureSetByIdQuery(TestTenantId.Value, featureSetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FeatureSetId.Should().Be(featureSetId);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenFeatureSetDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FeatureSetDetailDto?)null);

        var result = await Sut().Handle(new GetFeatureSetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing FeatureSet record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenFeatureSetBelongsToDifferentTenant_ReturnsNotFound()
    {
        var featureSetId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(featureSetId, otherTenantId, Arg.Any<CancellationToken>()).Returns(FeatureSetFor(featureSetId, otherTenantId));

        var result = await Sut().Handle(new GetFeatureSetByIdQuery(TestTenantId.Value, featureSetId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.NotFound");
    }
}
