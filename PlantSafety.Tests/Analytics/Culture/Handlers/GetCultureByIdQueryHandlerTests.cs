// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCultureByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCultureByIdQueryHandler -- Phase 4 Analytics.Culture full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.Culture.Handlers;
using PlantSafety.Application.Analytics.Culture.Models;
using PlantSafety.Application.Analytics.Culture.Queries;
using PlantSafety.Application.Analytics.Culture.Repositories;

namespace PlantSafety.Tests.Analytics.Culture.Handlers;

public sealed class GetCultureByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICultureReadRepository _repo = Substitute.For<ICultureReadRepository>();

    private GetCultureByIdQueryHandler Sut() => new(_repo, NullLogger<GetCultureByIdQueryHandler>());

    private static CultureItemDto CultureFor(Guid cultureId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        CultureId = cultureId,
        TenantId = tenantId,
        Status = "Created",
        SurveyPeriod = "2026-Q3",
        CultureIndexScore = 72.5m,
        EngagementScore = 68.0m,
        ParticipationRate = 81.0m,
        TopRiskTheme = "Fatigue",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenCultureExistsForTenant_ReturnsItemWithEtag()
    {
        var cultureId = Guid.NewGuid();
        var culture = CultureFor(cultureId, TestTenantId.Value);
        _repo.GetByIdAsync(cultureId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(culture);

        var result = await Sut().Handle(new GetCultureByIdQuery(TestTenantId.Value, cultureId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CultureId.Should().Be(cultureId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenCultureDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CultureItemDto?)null);

        var result = await Sut().Handle(new GetCultureByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Culture.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Culture record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenCultureBelongsToDifferentTenant_ReturnsNotFound()
    {
        var cultureId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(cultureId, otherTenantId, Arg.Any<CancellationToken>()).Returns(CultureFor(cultureId, otherTenantId));

        var result = await Sut().Handle(new GetCultureByIdQuery(TestTenantId.Value, cultureId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Culture.NotFound");
    }
}
