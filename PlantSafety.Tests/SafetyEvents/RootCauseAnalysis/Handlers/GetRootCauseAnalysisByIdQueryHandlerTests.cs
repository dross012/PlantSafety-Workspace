// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.RootCauseAnalysis.Handlers
//  Layer:          Tests (Unit)
//  File:           GetRootCauseAnalysisByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetRootCauseAnalysisByIdQueryHandler -- Phase 4
//                  SafetyEvents.RootCauseAnalysis full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a RootCauseAnalysis record belonging to a different tenant
//                  must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;

namespace PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Handlers;

public sealed class GetRootCauseAnalysisByIdQueryHandlerTests : UnitTestBase
{
    private readonly IRootCauseAnalysisRepository _repo = Substitute.For<IRootCauseAnalysisRepository>();

    private GetRootCauseAnalysisByIdQueryHandler Sut() => new(_repo, NullLogger<GetRootCauseAnalysisByIdQueryHandler>());

    private static RootCauseAnalysisDetailDto RootCauseAnalysisFor(Guid rootCauseAnalysisId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        RootCauseAnalysisId = rootCauseAnalysisId,
        TenantId = tenantId,
        Status = "InProgress",
        Methodology = "FiveWhys",
        LeadAnalyst = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenRootCauseAnalysisExistsForTenant_ReturnsItemWithEtag()
    {
        var rootCauseAnalysisId = Guid.NewGuid();
        var analysis = RootCauseAnalysisFor(rootCauseAnalysisId, TestTenantId.Value);
        _repo.GetByIdAsync(rootCauseAnalysisId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(analysis);

        var result = await Sut().Handle(new GetRootCauseAnalysisByIdQuery(TestTenantId.Value, rootCauseAnalysisId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RootCauseAnalysisId.Should().Be(rootCauseAnalysisId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenRootCauseAnalysisDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RootCauseAnalysisDetailDto?)null);

        var result = await Sut().Handle(new GetRootCauseAnalysisByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.RootCauseAnalysis.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing RootCauseAnalysis record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenRootCauseAnalysisBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_RootCauseAnalysis_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a RootCauseAnalysis record that actually belongs
        // to another tenant returns no row at all -- the repository is not stubbed for that
        // (RootCauseAnalysisId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var rootCauseAnalysisId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(rootCauseAnalysisId, otherTenantId, Arg.Any<CancellationToken>()).Returns(RootCauseAnalysisFor(rootCauseAnalysisId, otherTenantId));

        var result = await Sut().Handle(new GetRootCauseAnalysisByIdQuery(TestTenantId.Value, rootCauseAnalysisId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.RootCauseAnalysis.NotFound");
    }
}
