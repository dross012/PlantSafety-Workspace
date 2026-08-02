// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.CrisisManagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCrisisManagementByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCrisisManagementByIdQueryHandler -- Phase 4
//                  SafetyEvents.CrisisManagement full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a CrisisManagement record belonging to a different tenant
//                  must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Handlers;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;

namespace PlantSafety.Tests.SafetyEvents.CrisisManagement.Handlers;

public sealed class GetCrisisManagementByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICrisisManagementRepository _repo = Substitute.For<ICrisisManagementRepository>();

    private GetCrisisManagementByIdQueryHandler Sut() => new(_repo, NullLogger<GetCrisisManagementByIdQueryHandler>());

    private static CrisisManagementDetailDto CrisisFor(Guid crisisManagementId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        CrisisManagementId = crisisManagementId,
        TenantId = tenantId,
        Status = "Declared",
        Title = "Tank Farm Fire",
        Severity = "Critical",
        DeclaredAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenCrisisExistsForTenant_ReturnsItemWithEtag()
    {
        var crisisId = Guid.NewGuid();
        var crisis = CrisisFor(crisisId, TestTenantId.Value);
        _repo.GetByIdAsync(crisisId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(crisis);

        var result = await Sut().Handle(new GetCrisisManagementByIdQuery(TestTenantId.Value, crisisId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CrisisManagementId.Should().Be(crisisId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenCrisisDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CrisisManagementDetailDto?)null);

        var result = await Sut().Handle(new GetCrisisManagementByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.CrisisManagement.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing CrisisManagement record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenCrisisBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_CrisisManagement_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a crisis that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (crisisId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var crisisId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(crisisId, otherTenantId, Arg.Any<CancellationToken>()).Returns(CrisisFor(crisisId, otherTenantId));

        var result = await Sut().Handle(new GetCrisisManagementByIdQuery(TestTenantId.Value, crisisId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.CrisisManagement.NotFound");
    }
}
