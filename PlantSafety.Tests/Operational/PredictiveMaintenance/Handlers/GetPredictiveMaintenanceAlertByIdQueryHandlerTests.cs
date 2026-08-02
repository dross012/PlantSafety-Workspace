// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.PredictiveMaintenance.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPredictiveMaintenanceAlertByIdQueryHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPredictiveMaintenanceAlertByIdQueryHandler -- Phase 4
//                  Operational.PredictiveMaintenance full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a PredictiveMaintenanceAlert record belonging to a different
//                  tenant must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Tests.Operational.PredictiveMaintenance.Handlers;

public sealed class GetPredictiveMaintenanceAlertByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPredictiveMaintenanceAlertRepository _repo = Substitute.For<IPredictiveMaintenanceAlertRepository>();

    private GetPredictiveMaintenanceAlertByIdQueryHandler Sut() => new(_repo, NullLogger<GetPredictiveMaintenanceAlertByIdQueryHandler>());

    private static PredictiveMaintenanceAlertItemDto AlertFor(Guid predictiveMaintenanceAlertId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        PredictiveMaintenanceAlertId = predictiveMaintenanceAlertId,
        TenantId = tenantId,
        Status = "Raised",
        EquipmentId = Guid.NewGuid(),
        PredictedFailureType = "BearingFailure",
        PredictedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAlertExistsForTenant_ReturnsItemWithEtag()
    {
        var alertId = Guid.NewGuid();
        var alert = AlertFor(alertId, TestTenantId.Value);
        _repo.GetByIdAsync(alertId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(alert);

        var result = await Sut().Handle(new GetPredictiveMaintenanceAlertByIdQuery(TestTenantId.Value, alertId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PredictiveMaintenanceAlertId.Should().Be(alertId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenAlertDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PredictiveMaintenanceAlertItemDto?)null);

        var result = await Sut().Handle(new GetPredictiveMaintenanceAlertByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.PredictiveMaintenance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing PredictiveMaintenanceAlert record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAlertBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_PredictiveMaintenanceAlert_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for an alert that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (alertId, TestTenantId) combination and NSubstitute's default (null) applies, exactly as
        // the real proc would.
        var alertId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(alertId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AlertFor(alertId, otherTenantId));

        var result = await Sut().Handle(new GetPredictiveMaintenanceAlertByIdQuery(TestTenantId.Value, alertId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.PredictiveMaintenance.NotFound");
    }
}
