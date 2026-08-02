// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AiSafetyIntelligence.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAiSafetyIntelligenceByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAiSafetyIntelligenceByIdQueryHandler -- Phase 4
//                  AiSafetyIntelligence full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AiSafetyIntelligence.Handlers;
using PlantSafety.Application.AiSafetyIntelligence.Models;
using PlantSafety.Application.AiSafetyIntelligence.Queries;
using PlantSafety.Application.AiSafetyIntelligence.Repositories;

namespace PlantSafety.Tests.AiSafetyIntelligence.Handlers;

public sealed class GetAiSafetyIntelligenceByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAiSafetyIntelligenceRepository _repo = Substitute.For<IAiSafetyIntelligenceRepository>();

    private GetAiSafetyIntelligenceByIdQueryHandler Sut() => new(_repo, NullLogger<GetAiSafetyIntelligenceByIdQueryHandler>());

    private static AiSafetyIntelligenceItemDto AiSafetyIntelligenceFor(Guid aiSafetyIntelligenceId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        AiSafetyIntelligenceId = aiSafetyIntelligenceId,
        TenantId = tenantId,
        Status = "Created",
        InsightType = "AnomalyDetection",
        Description = "Unusual vibration pattern detected on Line 3 conveyor",
        ConfidenceScore = 0.87m,
        SourceModelName = "VibrationAnomalyModel-v3",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAiSafetyIntelligenceExistsForTenant_ReturnsItemWithEtag()
    {
        var aiSafetyIntelligenceId = Guid.NewGuid();
        var aiSafetyIntelligence = AiSafetyIntelligenceFor(aiSafetyIntelligenceId, TestTenantId.Value);
        _repo.GetByIdAsync(aiSafetyIntelligenceId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(aiSafetyIntelligence);

        var result = await Sut().Handle(new GetAiSafetyIntelligenceByIdQuery(TestTenantId.Value, aiSafetyIntelligenceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AiSafetyIntelligenceId.Should().Be(aiSafetyIntelligenceId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenAiSafetyIntelligenceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiSafetyIntelligenceItemDto?)null);

        var result = await Sut().Handle(new GetAiSafetyIntelligenceByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiSafetyIntelligence.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AiSafetyIntelligence record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAiSafetyIntelligenceBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_AiSafetyIntelligenceAggregate_GetById filters by
        // TenantId in SQL, so a lookup scoped to TestTenantId for a record that actually belongs
        // to another tenant returns no row at all -- the repository is not stubbed for that
        // (AiSafetyIntelligenceId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var aiSafetyIntelligenceId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(aiSafetyIntelligenceId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AiSafetyIntelligenceFor(aiSafetyIntelligenceId, otherTenantId));

        var result = await Sut().Handle(new GetAiSafetyIntelligenceByIdQuery(TestTenantId.Value, aiSafetyIntelligenceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiSafetyIntelligence.NotFound");
    }
}
