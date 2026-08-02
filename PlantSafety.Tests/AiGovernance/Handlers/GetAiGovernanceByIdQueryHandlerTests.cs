// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AiGovernance.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAiGovernanceByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAiGovernanceByIdQueryHandler -- Phase 4 AiGovernance full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AiGovernance.Handlers;
using PlantSafety.Application.AiGovernance.Models;
using PlantSafety.Application.AiGovernance.Queries;
using PlantSafety.Application.AiGovernance.Repositories;

namespace PlantSafety.Tests.AiGovernance.Handlers;

public sealed class GetAiGovernanceByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAiGovernanceRepository _repo = Substitute.For<IAiGovernanceRepository>();

    private GetAiGovernanceByIdQueryHandler Sut() => new(_repo, NullLogger<GetAiGovernanceByIdQueryHandler>());

    private static AiGovernanceItemDto AiGovernanceFor(Guid aiGovernanceId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        AiGovernanceId = aiGovernanceId,
        TenantId = tenantId,
        Status = "Created",
        SystemName = "Predictive Maintenance Model",
        AiUseCase = "Forecast equipment failure risk",
        RiskClassification = "High",
        OwnerUserId = TestUserId.Value,
        RequiresHumanOversight = true,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAiGovernanceExistsForTenant_ReturnsItemWithEtag()
    {
        var aiGovernanceId = Guid.NewGuid();
        var aiGovernance = AiGovernanceFor(aiGovernanceId, TestTenantId.Value);
        _repo.GetByIdAsync(aiGovernanceId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(aiGovernance);

        var result = await Sut().Handle(new GetAiGovernanceByIdQuery(TestTenantId.Value, aiGovernanceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AiGovernanceId.Should().Be(aiGovernanceId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenAiGovernanceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiGovernanceItemDto?)null);

        var result = await Sut().Handle(new GetAiGovernanceByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiGovernance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AiGovernance record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAiGovernanceBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_AiGovernanceAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (AiGovernanceId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var aiGovernanceId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(aiGovernanceId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AiGovernanceFor(aiGovernanceId, otherTenantId));

        var result = await Sut().Handle(new GetAiGovernanceByIdQuery(TestTenantId.Value, aiGovernanceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiGovernance.NotFound");
    }
}
