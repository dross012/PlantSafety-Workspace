// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnterpriseFabric.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEnterpriseFabricByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEnterpriseFabricByIdQueryHandler -- Phase 4 EnterpriseFabric
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.EnterpriseFabric.Handlers;
using PlantSafety.Application.EnterpriseFabric.Models;
using PlantSafety.Application.EnterpriseFabric.Queries;
using PlantSafety.Application.EnterpriseFabric.Repositories;

namespace PlantSafety.Tests.EnterpriseFabric.Handlers;

public sealed class GetEnterpriseFabricByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEnterpriseFabricRepository _repo = Substitute.For<IEnterpriseFabricRepository>();

    private GetEnterpriseFabricByIdQueryHandler Sut() => new(_repo, NullLogger<GetEnterpriseFabricByIdQueryHandler>());

    private static EnterpriseFabricItemDto FabricFor(Guid enterpriseFabricId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        EnterpriseFabricId = enterpriseFabricId,
        TenantId = tenantId,
        Status = "Created",
        FabricArea = "IntegrationHub",
        SystemReference = "SAP-ERP",
        IntegrationType = "Api",
        OwnerUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEnterpriseFabricExistsForTenant_ReturnsItemWithEtag()
    {
        var enterpriseFabricId = Guid.NewGuid();
        var fabric = FabricFor(enterpriseFabricId, TestTenantId.Value);
        _repo.GetByIdAsync(enterpriseFabricId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(fabric);

        var result = await Sut().Handle(new GetEnterpriseFabricByIdQuery(TestTenantId.Value, enterpriseFabricId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EnterpriseFabricId.Should().Be(enterpriseFabricId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenEnterpriseFabricDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EnterpriseFabricItemDto?)null);

        var result = await Sut().Handle(new GetEnterpriseFabricByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnterpriseFabric.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EnterpriseFabric record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEnterpriseFabricBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EnterpriseFabricAggregate_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for a record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (EnterpriseFabricId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var enterpriseFabricId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(enterpriseFabricId, otherTenantId, Arg.Any<CancellationToken>()).Returns(FabricFor(enterpriseFabricId, otherTenantId));

        var result = await Sut().Handle(new GetEnterpriseFabricByIdQuery(TestTenantId.Value, enterpriseFabricId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnterpriseFabric.NotFound");
    }
}
