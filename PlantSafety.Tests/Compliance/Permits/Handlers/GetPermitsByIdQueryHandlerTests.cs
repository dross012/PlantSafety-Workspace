// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPermitsByIdQueryHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPermitsByIdQueryHandler -- Phase 4 Compliance.Permits full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (a Permits record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.RiskManagement.Handlers.GetRiskByIdQueryHandlerTests /
//                  PlantSafety.Tests.AccidentManagement.Handlers.GetAccidentByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Permits.Handlers;
using PlantSafety.Application.Compliance.Permits.Models;
using PlantSafety.Application.Compliance.Permits.Queries;
using PlantSafety.Application.Compliance.Permits.Repositories;

namespace PlantSafety.Tests.Compliance.Permits.Handlers;

public sealed class GetPermitsByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPermitsRepository _repo = Substitute.For<IPermitsRepository>();

    private GetPermitsByIdQueryHandler Sut() => new(_repo, NullLogger<GetPermitsByIdQueryHandler>());

    private static PermitsItemDto PermitsFor(Guid permitsId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        PermitsId = permitsId,
        TenantId = tenantId,
        Status = "Created",
        PermitType = "HotWork",
        PermitNumber = "PTW-2026-0001",
        WorkDescription = "Welding on Line 3",
        Location = "Line 3",
        HazardsIdentified = "Fire, fumes",
        RequestedByUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenPermitsExistsForTenant_ReturnsItemWithEtag()
    {
        var permitsId = Guid.NewGuid();
        var permits = PermitsFor(permitsId, TestTenantId.Value);
        _repo.GetByIdAsync(permitsId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(permits);

        var result = await Sut().Handle(new GetPermitsByIdQuery(TestTenantId.Value, permitsId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PermitsId.Should().Be(permitsId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenPermitsDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PermitsItemDto?)null);

        var result = await Sut().Handle(new GetPermitsByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Permits.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Permits record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenPermitsBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_Permits_GetById filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for a Permits record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (PermitsId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var permitsId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(permitsId, otherTenantId, Arg.Any<CancellationToken>()).Returns(PermitsFor(permitsId, otherTenantId));

        var result = await Sut().Handle(new GetPermitsByIdQuery(TestTenantId.Value, permitsId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Permits.NotFound");
    }
}
