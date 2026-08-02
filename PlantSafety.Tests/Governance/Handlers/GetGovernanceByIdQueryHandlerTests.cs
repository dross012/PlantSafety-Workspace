// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Governance.Handlers
//  Layer:          Tests (Unit)
//  File:           GetGovernanceByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetGovernanceByIdQueryHandler -- Phase 4 Governance full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Governance.Handlers;
using PlantSafety.Application.Governance.Models;
using PlantSafety.Application.Governance.Queries;
using PlantSafety.Application.Governance.Repositories;

namespace PlantSafety.Tests.Governance.Handlers;

public sealed class GetGovernanceByIdQueryHandlerTests : UnitTestBase
{
    private readonly IGovernanceRepository _repo = Substitute.For<IGovernanceRepository>();

    private GetGovernanceByIdQueryHandler Sut() => new(_repo, NullLogger<GetGovernanceByIdQueryHandler>());

    private static GovernanceItemDto GovernanceFor(Guid governanceId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        GovernanceId = governanceId,
        TenantId = tenantId,
        Status = "Created",
        GovernanceArea = "PolicyOversight",
        ReviewCycleMonths = 12,
        OwnerUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenGovernanceExistsForTenant_ReturnsItemWithEtag()
    {
        var governanceId = Guid.NewGuid();
        var governance = GovernanceFor(governanceId, TestTenantId.Value);
        _repo.GetByIdAsync(governanceId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(governance);

        var result = await Sut().Handle(new GetGovernanceByIdQuery(TestTenantId.Value, governanceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.GovernanceId.Should().Be(governanceId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenGovernanceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((GovernanceItemDto?)null);

        var result = await Sut().Handle(new GetGovernanceByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Governance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Governance record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenGovernanceBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_GovernanceAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (GovernanceId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var governanceId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(governanceId, otherTenantId, Arg.Any<CancellationToken>()).Returns(GovernanceFor(governanceId, otherTenantId));

        var result = await Sut().Handle(new GetGovernanceByIdQuery(TestTenantId.Value, governanceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Governance.NotFound");
    }
}
