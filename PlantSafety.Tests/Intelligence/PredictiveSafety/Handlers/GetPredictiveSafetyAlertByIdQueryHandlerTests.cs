// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.PredictiveSafety.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPredictiveSafetyAlertByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPredictiveSafetyAlertByIdQueryHandler -- Phase 4
//                  Intelligence.PredictiveSafety full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and tenant-isolation
//                  (a PredictiveSafetyAlert record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Handlers.GetRCAByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.PredictiveSafety.Handlers;
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;
using PlantSafety.Application.Intelligence.PredictiveSafety.Queries;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

namespace PlantSafety.Tests.Intelligence.PredictiveSafety.Handlers;

public sealed class GetPredictiveSafetyAlertByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPredictiveSafetyAlertDetailRepository _repo = Substitute.For<IPredictiveSafetyAlertDetailRepository>();

    private GetPredictiveSafetyAlertByIdQueryHandler Sut() => new(_repo, NullLogger<GetPredictiveSafetyAlertByIdQueryHandler>());

    private static PredictiveSafetyAlertDetailDto AlertFor(Guid predictiveSafetyAlertId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        PredictiveSafetyAlertId = predictiveSafetyAlertId,
        TenantId = tenantId,
        RiskCategory = "Fatigue",
        PredictedRiskScore = 0.87,
        ModelVersion = "2.1.0",
        Status = "Active",
        PredictedAtUtc = FixedUtcNow,
        GeneratedBy = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenPredictiveSafetyAlertExistsForTenant_ReturnsItemWithEtag()
    {
        var predictiveSafetyAlertId = Guid.NewGuid();
        var alert = AlertFor(predictiveSafetyAlertId, TestTenantId.Value);
        _repo.GetByIdAsync(predictiveSafetyAlertId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(alert);

        var result = await Sut().Handle(new GetPredictiveSafetyAlertByIdQuery(TestTenantId.Value, predictiveSafetyAlertId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PredictiveSafetyAlertId.Should().Be(predictiveSafetyAlertId);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenPredictiveSafetyAlertDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PredictiveSafetyAlertDetailDto?)null);

        var result = await Sut().Handle(new GetPredictiveSafetyAlertByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.PredictiveSafety.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing PredictiveSafetyAlert record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenPredictiveSafetyAlertBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_PredictiveSafetyAlert_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a PredictiveSafetyAlert record that actually
        // belongs to another tenant returns no row at all -- the repository is not stubbed for
        // that (predictiveSafetyAlertId, TestTenantId) combination and NSubstitute's default
        // (null) applies, exactly as the real proc would.
        var predictiveSafetyAlertId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(predictiveSafetyAlertId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AlertFor(predictiveSafetyAlertId, otherTenantId));

        var result = await Sut().Handle(new GetPredictiveSafetyAlertByIdQuery(TestTenantId.Value, predictiveSafetyAlertId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.PredictiveSafety.NotFound");
    }
}
