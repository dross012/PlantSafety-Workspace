// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ContinuousImprovement.RCA.Handlers
//  Layer:          Tests (Unit)
//  File:           GetRCAByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetRCAByIdQueryHandler -- Phase 4 ContinuousImprovement.RCA
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (an RCA record belonging
//                  to a different tenant must surface as NotFound, never leaking cross-tenant
//                  existence). Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ContinuousImprovement.RCA.Handlers;
using PlantSafety.Application.ContinuousImprovement.RCA.Models;
using PlantSafety.Application.ContinuousImprovement.RCA.Queries;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

namespace PlantSafety.Tests.ContinuousImprovement.RCA.Handlers;

public sealed class GetRCAByIdQueryHandlerTests : UnitTestBase
{
    private readonly IRCADetailRepository _repo = Substitute.For<IRCADetailRepository>();

    private GetRCAByIdQueryHandler Sut() => new(_repo, NullLogger<GetRCAByIdQueryHandler>());

    private static RCADetailDto RCAFor(Guid rcaId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        RcaId = rcaId,
        TenantId = tenantId,
        ProblemStatement = "Unplanned line stoppage on Packaging Line 3",
        Methodology = "FiveWhys",
        Status = "Initiated",
        InitiatedAtUtc = FixedUtcNow,
        OwnerUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenRCAExistsForTenant_ReturnsItemWithEtag()
    {
        var rcaId = Guid.NewGuid();
        var rca = RCAFor(rcaId, TestTenantId.Value);
        _repo.GetByIdAsync(rcaId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(rca);

        var result = await Sut().Handle(new GetRCAByIdQuery(TestTenantId.Value, rcaId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RcaId.Should().Be(rcaId);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenRCADoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RCADetailDto?)null);

        var result = await Sut().Handle(new GetRCAByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContinuousImprovement.RCA.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing RCA record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenRCABelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_RCA_GetById filters by TenantId in SQL, so a lookup
        // scoped to TestTenantId for an RCA record that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that (rcaId, TestTenantId)
        // combination and NSubstitute's default (null) applies, exactly as the real proc would.
        var rcaId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(rcaId, otherTenantId, Arg.Any<CancellationToken>()).Returns(RCAFor(rcaId, otherTenantId));

        var result = await Sut().Handle(new GetRCAByIdQuery(TestTenantId.Value, rcaId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContinuousImprovement.RCA.NotFound");
    }
}
