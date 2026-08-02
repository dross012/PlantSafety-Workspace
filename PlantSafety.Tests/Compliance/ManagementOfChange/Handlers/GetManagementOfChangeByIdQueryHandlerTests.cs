// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.ManagementOfChange.Handlers
//  Layer:          Tests (Unit)
//  File:           GetManagementOfChangeByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetManagementOfChangeByIdQueryHandler -- Phase 4
//                  Compliance.ManagementOfChange full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and tenant-isolation
//                  (a record belonging to a different tenant must surface as NotFound, never
//                  leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.ManagementOfChange.Handlers;
using PlantSafety.Application.Compliance.ManagementOfChange.Models;
using PlantSafety.Application.Compliance.ManagementOfChange.Queries;
using PlantSafety.Application.Compliance.ManagementOfChange.Repositories;

namespace PlantSafety.Tests.Compliance.ManagementOfChange.Handlers;

public sealed class GetManagementOfChangeByIdQueryHandlerTests : UnitTestBase
{
    private readonly IManagementOfChangeRepository _repo = Substitute.For<IManagementOfChangeRepository>();

    private GetManagementOfChangeByIdQueryHandler Sut() => new(_repo, NullLogger<GetManagementOfChangeByIdQueryHandler>());

    private static ManagementOfChangeItemDto ManagementOfChangeFor(Guid managementOfChangeId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ManagementOfChangeId = managementOfChangeId,
        TenantId = tenantId,
        Status = "Created",
        ChangeTitle = "Replace conveyor guard",
        ChangeType = "Equipment",
        Justification = "Worn guard poses pinch hazard",
        RiskLevel = "Medium",
        RequestedByUserId = TestUserId.Value,
        ApprovalStatus = "Pending",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenManagementOfChangeExistsForTenant_ReturnsItemWithEtag()
    {
        var managementOfChangeId = Guid.NewGuid();
        var moc = ManagementOfChangeFor(managementOfChangeId, TestTenantId.Value);
        _repo.GetByIdAsync(managementOfChangeId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(moc);

        var result = await Sut().Handle(new GetManagementOfChangeByIdQuery(TestTenantId.Value, managementOfChangeId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ManagementOfChangeId.Should().Be(managementOfChangeId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenManagementOfChangeDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ManagementOfChangeItemDto?)null);

        var result = await Sut().Handle(new GetManagementOfChangeByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.ManagementOfChange.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ManagementOfChange record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenManagementOfChangeBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_ManagementOfChange_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a record that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that
        // (ManagementOfChangeId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var managementOfChangeId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(managementOfChangeId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ManagementOfChangeFor(managementOfChangeId, otherTenantId));

        var result = await Sut().Handle(new GetManagementOfChangeByIdQuery(TestTenantId.Value, managementOfChangeId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.ManagementOfChange.NotFound");
    }
}
