// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.Handlers
//  Layer:          Tests (Unit)
//  File:           GetHumanFactorsByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetHumanFactorsByIdQueryHandler -- Phase 4 HumanFactors full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.Handlers;
using PlantSafety.Application.HumanFactors.Models;
using PlantSafety.Application.HumanFactors.Queries;
using PlantSafety.Application.HumanFactors.Repositories;

namespace PlantSafety.Tests.HumanFactors.Handlers;

public sealed class GetHumanFactorsByIdQueryHandlerTests : UnitTestBase
{
    private readonly IHumanFactorsRepository _repo = Substitute.For<IHumanFactorsRepository>();

    private GetHumanFactorsByIdQueryHandler Sut() => new(_repo, NullLogger<GetHumanFactorsByIdQueryHandler>());

    private static HumanFactorsItemDto HumanFactorsFor(Guid humanFactorsId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        HumanFactorsId = humanFactorsId,
        TenantId = tenantId,
        Status = "Created",
        FactorType = "Fatigue",
        SeverityRating = 3,
        Description = "Extended overtime shift pattern flagged for review.",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenHumanFactorsExistsForTenant_ReturnsItemWithEtag()
    {
        var humanFactorsId = Guid.NewGuid();
        var humanFactors = HumanFactorsFor(humanFactorsId, TestTenantId.Value);
        _repo.GetByIdAsync(humanFactorsId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(humanFactors);

        var result = await Sut().Handle(new GetHumanFactorsByIdQuery(TestTenantId.Value, humanFactorsId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.HumanFactorsId.Should().Be(humanFactorsId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenHumanFactorsDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HumanFactorsItemDto?)null);

        var result = await Sut().Handle(new GetHumanFactorsByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing HumanFactors record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenHumanFactorsBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_HumanFactorsAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (HumanFactorsId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var humanFactorsId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(humanFactorsId, otherTenantId, Arg.Any<CancellationToken>()).Returns(HumanFactorsFor(humanFactorsId, otherTenantId));

        var result = await Sut().Handle(new GetHumanFactorsByIdQuery(TestTenantId.Value, humanFactorsId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.NotFound");
    }
}
