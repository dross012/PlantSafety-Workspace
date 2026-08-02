// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ContractorVisitor.Handlers
//  Layer:          Tests (Unit)
//  File:           GetContractorVisitorByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetContractorVisitorByIdQueryHandler -- Phase 4
//                  ContractorVisitor full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ContractorVisitor.Handlers;
using PlantSafety.Application.ContractorVisitor.Models;
using PlantSafety.Application.ContractorVisitor.Queries;
using PlantSafety.Application.ContractorVisitor.Repositories;

namespace PlantSafety.Tests.ContractorVisitor.Handlers;

public sealed class GetContractorVisitorByIdQueryHandlerTests : UnitTestBase
{
    private readonly IContractorVisitorRepository _repo = Substitute.For<IContractorVisitorRepository>();

    private GetContractorVisitorByIdQueryHandler Sut() => new(_repo, NullLogger<GetContractorVisitorByIdQueryHandler>());

    private static ContractorVisitorItemDto ContractorVisitorFor(Guid contractorVisitorId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ContractorVisitorId = contractorVisitorId,
        TenantId = tenantId,
        Status = "Created",
        FullName = "Jordan Smith",
        CompanyName = "Acme Contracting LLC",
        VisitorType = "Contractor",
        PurposeOfVisit = "Scheduled HVAC maintenance",
        HostUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenContractorVisitorExistsForTenant_ReturnsItemWithEtag()
    {
        var contractorVisitorId = Guid.NewGuid();
        var contractorVisitor = ContractorVisitorFor(contractorVisitorId, TestTenantId.Value);
        _repo.GetByIdAsync(contractorVisitorId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(contractorVisitor);

        var result = await Sut().Handle(new GetContractorVisitorByIdQuery(TestTenantId.Value, contractorVisitorId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ContractorVisitorId.Should().Be(contractorVisitorId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenContractorVisitorDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ContractorVisitorItemDto?)null);

        var result = await Sut().Handle(new GetContractorVisitorByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContractorVisitor.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ContractorVisitor record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenContractorVisitorBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_ContractorVisitorAggregate_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for a record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (ContractorVisitorId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var contractorVisitorId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(contractorVisitorId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ContractorVisitorFor(contractorVisitorId, otherTenantId));

        var result = await Sut().Handle(new GetContractorVisitorByIdQuery(TestTenantId.Value, contractorVisitorId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContractorVisitor.NotFound");
    }
}
