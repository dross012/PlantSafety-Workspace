// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AIInvestigations.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAIInvestigationByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAIInvestigationByIdQueryHandler -- Phase 4
//                  Intelligence.AIInvestigations full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and tenant-isolation
//                  (an AIInvestigation record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandlerTests. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.AIInvestigations.Handlers;
using PlantSafety.Application.Intelligence.AIInvestigations.Models;
using PlantSafety.Application.Intelligence.AIInvestigations.Queries;
using PlantSafety.Application.Intelligence.AIInvestigations.Repositories;

namespace PlantSafety.Tests.Intelligence.AIInvestigations.Handlers;

public sealed class GetAIInvestigationByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAIInvestigationReadRepository _repo = Substitute.For<IAIInvestigationReadRepository>();

    private GetAIInvestigationByIdQueryHandler Sut() => new(_repo, NullLogger<GetAIInvestigationByIdQueryHandler>());

    private static AIInvestigationDetailDto InvestigationFor(Guid aiInvestigationId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        AIInvestigationId = aiInvestigationId,
        TenantId = tenantId,
        Status = "Open",
        IncidentReference = "INC-2026-0001",
        AiConfidenceScore = 0.87,
        AssignedTo = TestUserId.Value,
        OpenedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAIInvestigationExistsForTenant_ReturnsItemWithEtag()
    {
        var aiInvestigationId = Guid.NewGuid();
        var investigation = InvestigationFor(aiInvestigationId, TestTenantId.Value);
        _repo.GetByIdAsync(aiInvestigationId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(investigation);

        var result = await Sut().Handle(new GetAIInvestigationByIdQuery(TestTenantId.Value, aiInvestigationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AIInvestigationId.Should().Be(aiInvestigationId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenAIInvestigationDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AIInvestigationDetailDto?)null);

        var result = await Sut().Handle(new GetAIInvestigationByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AIInvestigation.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AIInvestigation record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAIInvestigationBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_AIInvestigation_GetById filters by TenantId in SQL, so
        // a lookup scoped to TestTenantId for an AIInvestigation record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (AIInvestigationId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var aiInvestigationId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(aiInvestigationId, otherTenantId, Arg.Any<CancellationToken>()).Returns(InvestigationFor(aiInvestigationId, otherTenantId));

        var result = await Sut().Handle(new GetAIInvestigationByIdQuery(TestTenantId.Value, aiInvestigationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AIInvestigation.NotFound");
    }
}
