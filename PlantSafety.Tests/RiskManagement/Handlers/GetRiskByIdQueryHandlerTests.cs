// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.RiskManagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetRiskByIdQueryHandlerTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetRiskByIdQueryHandler -- closes the Phase 4 RiskManagement
//                  read-model gap. Covers: successful retrieval with ETag projection, not-found
//                  behavior, and tenant-isolation (a Risk belonging to a different tenant must
//                  surface as NotFound, never leaking cross-tenant existence).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.RiskManagement.Handlers;
using PlantSafety.Application.RiskManagement.Models;
using PlantSafety.Application.RiskManagement.Queries;
using PlantSafety.Application.RiskManagement.Repositories;

namespace PlantSafety.Tests.RiskManagement.Handlers;

public sealed class GetRiskByIdQueryHandlerTests : UnitTestBase
{
    private readonly IRiskRepository _repo = Substitute.For<IRiskRepository>();

    private GetRiskByIdQueryHandler Sut() => new(_repo, NullLogger<GetRiskByIdQueryHandler>());

    private static RiskItemDto RiskFor(Guid riskId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        RiskId = riskId,
        TenantId = tenantId,
        Status = "Identified",
        RiskTitle = "Unguarded conveyor pinch point",
        Category = "Medium",
        Location = "Line 3",
        HazardSource = "Mechanical",
        OwnerId = TestUserId.Value,
        ReviewCadence = "Quarterly",
        ReviewRequired = true,
        ReviewCount = 0,
        RiskVersion = 1,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenRiskExistsForTenant_ReturnsItemWithEtag()
    {
        var riskId = Guid.NewGuid();
        var risk = RiskFor(riskId, TestTenantId.Value);
        _repo.GetByIdAsync(riskId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(risk);

        var result = await Sut().Handle(new GetRiskByIdQuery(TestTenantId.Value, riskId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RiskId.Should().Be(riskId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenRiskDoesNotExist_ReturnsNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RiskItemDto?)null);

        var result = await Sut().Handle(new GetRiskByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.NotFound");
    }

    [Fact]
    public async Task Handle_WhenRiskBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_Risk_GetById now filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for a Risk that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that (RiskId, TestTenantId)
        // combination and NSubstitute's default (null) applies, exactly as the real proc would.
        var riskId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(riskId, otherTenantId, Arg.Any<CancellationToken>()).Returns(RiskFor(riskId, otherTenantId));

        var result = await Sut().Handle(new GetRiskByIdQuery(TestTenantId.Value, riskId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.NotFound");
    }
}
