// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.NearMisses.Handlers
//  Layer:          Tests (Unit)
//  File:           GetNearMissesByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetNearMissesByIdQueryHandler -- Phase 4 SafetyEvents.NearMisses
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (a NearMisses record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.NearMisses.Handlers;
using PlantSafety.Application.SafetyEvents.NearMisses.Models;
using PlantSafety.Application.SafetyEvents.NearMisses.Queries;
using PlantSafety.Application.SafetyEvents.NearMisses.Repositories;

namespace PlantSafety.Tests.SafetyEvents.NearMisses.Handlers;

public sealed class GetNearMissesByIdQueryHandlerTests : UnitTestBase
{
    private readonly INearMissesRepository _repo = Substitute.For<INearMissesRepository>();

    private GetNearMissesByIdQueryHandler Sut() => new(_repo, NullLogger<GetNearMissesByIdQueryHandler>());

    private static NearMissesDetailDto NearMissesFor(Guid nearMissesId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        NearMissesId = nearMissesId,
        TenantId = tenantId,
        Status = "Reported",
        Category = "SlipTripFall",
        PotentialSeverity = "Medium",
        ReportedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenNearMissesExistsForTenant_ReturnsItemWithEtag()
    {
        var nearMissesId = Guid.NewGuid();
        var nearMisses = NearMissesFor(nearMissesId, TestTenantId.Value);
        _repo.GetByIdAsync(nearMissesId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(nearMisses);

        var result = await Sut().Handle(new GetNearMissesByIdQuery(TestTenantId.Value, nearMissesId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NearMissesId.Should().Be(nearMissesId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenNearMissesDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NearMissesDetailDto?)null);

        var result = await Sut().Handle(new GetNearMissesByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.NearMisses.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing NearMisses record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenNearMissesBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_NearMisses_GetById filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for a NearMisses record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (NearMissesId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var nearMissesId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(nearMissesId, otherTenantId, Arg.Any<CancellationToken>()).Returns(NearMissesFor(nearMissesId, otherTenantId));

        var result = await Sut().Handle(new GetNearMissesByIdQuery(TestTenantId.Value, nearMissesId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.NearMisses.NotFound");
    }
}
