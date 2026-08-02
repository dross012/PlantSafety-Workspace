// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.Handlers
//  Layer:          Tests (Unit)
//  File:           GetOperationalSafetyByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetOperationalSafetyByIdQueryHandler -- Phase 4
//                  OperationalSafety full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.OperationalSafety.Handlers;
using PlantSafety.Application.OperationalSafety.Models;
using PlantSafety.Application.OperationalSafety.Queries;
using PlantSafety.Application.OperationalSafety.Repositories;

namespace PlantSafety.Tests.OperationalSafety.Handlers;

public sealed class GetOperationalSafetyByIdQueryHandlerTests : UnitTestBase
{
    private readonly IOperationalSafetyRepository _repo = Substitute.For<IOperationalSafetyRepository>();

    private GetOperationalSafetyByIdQueryHandler Sut() => new(_repo, NullLogger<GetOperationalSafetyByIdQueryHandler>());

    private static OperationalSafetyItemDto OperationalSafetyFor(Guid operationalSafetyId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        OperationalSafetyId = operationalSafetyId,
        TenantId = tenantId,
        Status = "Created",
        SafetyDomain = "LineOfFire",
        RiskLevel = "Medium",
        OwnerUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenOperationalSafetyExistsForTenant_ReturnsItemWithEtag()
    {
        var operationalSafetyId = Guid.NewGuid();
        var operationalSafety = OperationalSafetyFor(operationalSafetyId, TestTenantId.Value);
        _repo.GetByIdAsync(operationalSafetyId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(operationalSafety);

        var result = await Sut().Handle(new GetOperationalSafetyByIdQuery(TestTenantId.Value, operationalSafetyId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OperationalSafetyId.Should().Be(operationalSafetyId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenOperationalSafetyDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((OperationalSafetyItemDto?)null);

        var result = await Sut().Handle(new GetOperationalSafetyByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("OperationalSafety.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing OperationalSafety record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenOperationalSafetyBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_OperationalSafetyAggregate_GetById filters by
        // TenantId in SQL, so a lookup scoped to TestTenantId for a record that actually
        // belongs to another tenant returns no row at all -- the repository is not stubbed for
        // that (OperationalSafetyId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var operationalSafetyId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(operationalSafetyId, otherTenantId, Arg.Any<CancellationToken>()).Returns(OperationalSafetyFor(operationalSafetyId, otherTenantId));

        var result = await Sut().Handle(new GetOperationalSafetyByIdQuery(TestTenantId.Value, operationalSafetyId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("OperationalSafety.NotFound");
    }
}
