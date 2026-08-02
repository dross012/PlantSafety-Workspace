// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Compliance.Handlers
//  Layer:          Tests (Unit)
//  File:           GetComplianceByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetComplianceByIdQueryHandler -- Phase 4 (self-named)
//                  Compliance.Compliance full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Compliance.Handlers;
using PlantSafety.Application.Compliance.Compliance.Models;
using PlantSafety.Application.Compliance.Compliance.Queries;
using PlantSafety.Application.Compliance.Compliance.Repositories;

namespace PlantSafety.Tests.Compliance.Compliance.Handlers;

public sealed class GetComplianceByIdQueryHandlerTests : UnitTestBase
{
    private readonly IComplianceRepository _repo = Substitute.For<IComplianceRepository>();

    private GetComplianceByIdQueryHandler Sut() => new(_repo, NullLogger<GetComplianceByIdQueryHandler>());

    private static ComplianceItemDto ComplianceFor(Guid complianceId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ComplianceId = complianceId,
        TenantId = tenantId,
        Status = "Created",
        RegulationReference = "29 CFR 1910.147",
        RuleDescription = "Lockout/tagout program compliance",
        ResponsiblePartyId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenComplianceExistsForTenant_ReturnsItemWithEtag()
    {
        var complianceId = Guid.NewGuid();
        var compliance = ComplianceFor(complianceId, TestTenantId.Value);
        _repo.GetByIdAsync(complianceId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(compliance);

        var result = await Sut().Handle(new GetComplianceByIdQuery(TestTenantId.Value, complianceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ComplianceId.Should().Be(complianceId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenComplianceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ComplianceItemDto?)null);

        var result = await Sut().Handle(new GetComplianceByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Compliance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Compliance record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenComplianceBelongsToDifferentTenant_ReturnsNotFound()
    {
        var complianceId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(complianceId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ComplianceFor(complianceId, otherTenantId));

        var result = await Sut().Handle(new GetComplianceByIdQuery(TestTenantId.Value, complianceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Compliance.NotFound");
    }
}
