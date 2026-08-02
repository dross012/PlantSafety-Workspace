// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Financials.Handlers
//  Layer:          Tests (Unit)
//  File:           GetFinancialRecordByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for FinancialRecordByIdQueryHandler -- Phase 4 Financials full
//                  build-out (new FinancialRecord aggregate root). Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Financials.Handlers;
using PlantSafety.Application.Financials.Models;
using PlantSafety.Application.Financials.Queries;
using PlantSafety.Application.Financials.Repositories;

namespace PlantSafety.Tests.Financials.Handlers;

public sealed class GetFinancialRecordByIdQueryHandlerTests : UnitTestBase
{
    private readonly IFinancialRecordDetailRepository _repo = Substitute.For<IFinancialRecordDetailRepository>();

    private FinancialRecordByIdQueryHandler Sut() => new(_repo, NullLogger<FinancialRecordByIdQueryHandler>());

    private static FinancialRecordDetailDto RecordFor(Guid financialRecordId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        FinancialRecordId = financialRecordId,
        TenantId = tenantId,
        Status = "Draft",
        Category = "Budget",
        Amount = 1000m,
        LineItems = Array.Empty<FinancialRecordLineItemDto>(),
        RecordedAtUtc = FixedUtcNow,
        RecordedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenFinancialRecordExistsForTenant_ReturnsItemWithEtag()
    {
        var financialRecordId = Guid.NewGuid();
        var record = RecordFor(financialRecordId, TestTenantId.Value);
        _repo.GetByIdAsync(financialRecordId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(record);

        var result = await Sut().Handle(new FinancialRecordByIdQuery(TestTenantId.Value, financialRecordId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FinancialRecordId.Should().Be(financialRecordId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenFinancialRecordDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FinancialRecordDetailDto?)null);

        var result = await Sut().Handle(new FinancialRecordByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Financials.FinancialRecord.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing FinancialRecord must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenFinancialRecordBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_FinancialRecord_GetById filters by TenantId in SQL, so
        // a lookup scoped to TestTenantId for a record that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that
        // (FinancialRecordId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var financialRecordId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(financialRecordId, otherTenantId, Arg.Any<CancellationToken>()).Returns(RecordFor(financialRecordId, otherTenantId));

        var result = await Sut().Handle(new FinancialRecordByIdQuery(TestTenantId.Value, financialRecordId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Financials.FinancialRecord.NotFound");
    }
}
