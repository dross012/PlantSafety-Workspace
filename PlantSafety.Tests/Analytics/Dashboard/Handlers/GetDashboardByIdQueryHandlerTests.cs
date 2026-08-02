// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Dashboard.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDashboardByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDashboardByIdQueryHandler -- Phase 4 Analytics.Dashboard full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.Dashboard.Handlers;
using PlantSafety.Application.Analytics.Dashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.Repositories;

namespace PlantSafety.Tests.Analytics.Dashboard.Handlers;

public sealed class GetDashboardByIdQueryHandlerTests : UnitTestBase
{
    private readonly IDashboardReadRepository _repo = Substitute.For<IDashboardReadRepository>();

    private GetDashboardByIdQueryHandler Sut() => new(_repo, NullLogger<GetDashboardByIdQueryHandler>());

    private static DashboardItemDto DashboardFor(Guid dashboardId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        DashboardId = dashboardId,
        TenantId = tenantId,
        Status = "Created",
        DashboardName = "Plant Safety Overview",
        DashboardType = "Executive",
        OwnerId = TestUserId.Value,
        RefreshIntervalSeconds = 60,
        WidgetCount = 6,
        IsPublished = false,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenDashboardExistsForTenant_ReturnsItemWithEtag()
    {
        var dashboardId = Guid.NewGuid();
        var dashboard = DashboardFor(dashboardId, TestTenantId.Value);
        _repo.GetByIdAsync(dashboardId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(dashboard);

        var result = await Sut().Handle(new GetDashboardByIdQuery(TestTenantId.Value, dashboardId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DashboardId.Should().Be(dashboardId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenDashboardDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DashboardItemDto?)null);

        var result = await Sut().Handle(new GetDashboardByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Dashboard.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Dashboard record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenDashboardBelongsToDifferentTenant_ReturnsNotFound()
    {
        var dashboardId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(dashboardId, otherTenantId, Arg.Any<CancellationToken>()).Returns(DashboardFor(dashboardId, otherTenantId));

        var result = await Sut().Handle(new GetDashboardByIdQuery(TestTenantId.Value, dashboardId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Analytics.Dashboard.NotFound");
    }
}
