// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Engagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEngagementByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEngagementByIdQueryHandler -- Phase 4 Analytics.Engagement
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (an Engagement record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.Engagement.Handlers;
using PlantSafety.Application.Analytics.Engagement.Models;
using PlantSafety.Application.Analytics.Engagement.Queries;
using PlantSafety.Application.Analytics.Engagement.Repositories;

namespace PlantSafety.Tests.Analytics.Engagement.Handlers;

public sealed class GetEngagementByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEngagementRepository _repo = Substitute.For<IEngagementRepository>();

    private GetEngagementByIdQueryHandler Sut() => new(_repo, NullLogger<GetEngagementByIdQueryHandler>());

    private static EngagementItemDto EngagementFor(Guid engagementId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        EngagementId = engagementId,
        TenantId = tenantId,
        Status = "Created",
        WorkerId = TestUserId.Value,
        EngagementScore = 72.5m,
        SurveyPeriod = "2026-Q3",
        ParticipationChannel = "MobileApp",
        ResponseCount = 3,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEngagementExistsForTenant_ReturnsItemWithEtag()
    {
        var engagementId = Guid.NewGuid();
        var engagement = EngagementFor(engagementId, TestTenantId.Value);
        _repo.GetByIdAsync(engagementId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(engagement);

        var result = await Sut().Handle(new GetEngagementByIdQuery(TestTenantId.Value, engagementId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EngagementId.Should().Be(engagementId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenEngagementDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EngagementItemDto?)null);

        var result = await Sut().Handle(new GetEngagementByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Engagement.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Engagement record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEngagementBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EngagementRecords_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for an Engagement record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (engagementId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var engagementId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(engagementId, otherTenantId, Arg.Any<CancellationToken>()).Returns(EngagementFor(engagementId, otherTenantId));

        var result = await Sut().Handle(new GetEngagementByIdQuery(TestTenantId.Value, engagementId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Engagement.NotFound");
    }
}
