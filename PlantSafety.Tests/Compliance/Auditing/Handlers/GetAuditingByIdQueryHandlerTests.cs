// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Auditing.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAuditingByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAuditingByIdQueryHandler -- Phase 4 Compliance.Auditing full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Auditing.Handlers;
using PlantSafety.Application.Compliance.Auditing.Models;
using PlantSafety.Application.Compliance.Auditing.Queries;
using PlantSafety.Application.Compliance.Auditing.Repositories;

namespace PlantSafety.Tests.Compliance.Auditing.Handlers;

public sealed class GetAuditingByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAuditingRepository _repo = Substitute.For<IAuditingRepository>();

    private GetAuditingByIdQueryHandler Sut() => new(_repo, NullLogger<GetAuditingByIdQueryHandler>());

    private static AuditingItemDto AuditingFor(Guid auditingId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        AuditingId = auditingId,
        TenantId = tenantId,
        Status = "Created",
        AuditTitle = "Q3 Internal Safety Audit",
        AuditType = "Internal",
        Scope = "Line 3 confined space program",
        AuditorId = TestUserId.Value,
        ComplianceRating = "NotRated",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAuditingExistsForTenant_ReturnsItemWithEtag()
    {
        var auditingId = Guid.NewGuid();
        var auditing = AuditingFor(auditingId, TestTenantId.Value);
        _repo.GetByIdAsync(auditingId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(auditing);

        var result = await Sut().Handle(new GetAuditingByIdQuery(TestTenantId.Value, auditingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuditingId.Should().Be(auditingId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenAuditingDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AuditingItemDto?)null);

        var result = await Sut().Handle(new GetAuditingByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Auditing.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Auditing record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAuditingBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_AuditingAggregate_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (AuditingId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var auditingId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(auditingId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AuditingFor(auditingId, otherTenantId));

        var result = await Sut().Handle(new GetAuditingByIdQuery(TestTenantId.Value, auditingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Auditing.NotFound");
    }
}
