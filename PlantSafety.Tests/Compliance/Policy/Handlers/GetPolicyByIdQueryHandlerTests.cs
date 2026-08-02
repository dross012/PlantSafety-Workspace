// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Policy.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPolicyByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPolicyByIdQueryHandler -- Phase 4 Compliance.Policy full
//                  build-out. Covers: successful retrieval with ETag projection, not-found behavior
//                  with correct severity, and tenant-isolation (a Policy record belonging to a
//                  different tenant must surface as NotFound, never leaking cross-tenant
//                  existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Policy.Handlers;
using PlantSafety.Application.Compliance.Policy.Models;
using PlantSafety.Application.Compliance.Policy.Queries;
using PlantSafety.Application.Compliance.Policy.Repositories;

namespace PlantSafety.Tests.Compliance.Policy.Handlers;

public sealed class GetPolicyByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPolicyRepository _repo = Substitute.For<IPolicyRepository>();

    private GetPolicyByIdQueryHandler Sut() => new(_repo, NullLogger<GetPolicyByIdQueryHandler>());

    private static PolicyItemDto PolicyFor(Guid policyId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        PolicyId = policyId,
        TenantId = tenantId,
        Status = "Created",
        PolicyTitle = "Confined Space Entry Policy",
        PolicyVersion = 1,
        Category = "ConfinedSpace",
        OwnerId = TestUserId.Value,
        AcknowledgmentRequired = true,
        AcknowledgmentCount = 0,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenPolicyExistsForTenant_ReturnsItemWithEtag()
    {
        var policyId = Guid.NewGuid();
        var policy = PolicyFor(policyId, TestTenantId.Value);
        _repo.GetByIdAsync(policyId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(policy);

        var result = await Sut().Handle(new GetPolicyByIdQuery(TestTenantId.Value, policyId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PolicyId.Should().Be(policyId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenPolicyDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PolicyItemDto?)null);

        var result = await Sut().Handle(new GetPolicyByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Policy.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Policy record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenPolicyBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_Policy_GetById filters by TenantId in SQL, so a lookup
        // scoped to TestTenantId for a Policy record that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that
        // (PolicyId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var policyId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(policyId, otherTenantId, Arg.Any<CancellationToken>()).Returns(PolicyFor(policyId, otherTenantId));

        var result = await Sut().Handle(new GetPolicyByIdQuery(TestTenantId.Value, policyId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Policy.NotFound");
    }
}
