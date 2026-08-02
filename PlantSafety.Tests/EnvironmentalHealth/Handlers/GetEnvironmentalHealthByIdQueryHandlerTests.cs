// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEnvironmentalHealthByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEnvironmentalHealthByIdQueryHandler -- Phase 4
//                  EnvironmentalHealth full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.EnvironmentalHealth.Handlers;
using PlantSafety.Application.EnvironmentalHealth.Models;
using PlantSafety.Application.EnvironmentalHealth.Queries;
using PlantSafety.Application.EnvironmentalHealth.Repositories;

namespace PlantSafety.Tests.EnvironmentalHealth.Handlers;

public sealed class GetEnvironmentalHealthByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEnvironmentalHealthRepository _repo = Substitute.For<IEnvironmentalHealthRepository>();

    private GetEnvironmentalHealthByIdQueryHandler Sut() => new(_repo, NullLogger<GetEnvironmentalHealthByIdQueryHandler>());

    private static EnvironmentalHealthItemDto EnvironmentalHealthFor(Guid environmentalHealthId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        EnvironmentalHealthId = environmentalHealthId,
        TenantId = tenantId,
        Status = "Created",
        MonitoringType = "AirQuality",
        MeasuredValue = 42.5m,
        Unit = "AQI",
        Threshold = 100m,
        ThresholdExceeded = false,
        EvaluatedStatus = "Normal",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEnvironmentalHealthExistsForTenant_ReturnsItemWithEtag()
    {
        var environmentalHealthId = Guid.NewGuid();
        var environmentalHealth = EnvironmentalHealthFor(environmentalHealthId, TestTenantId.Value);
        _repo.GetByIdAsync(environmentalHealthId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(environmentalHealth);

        var result = await Sut().Handle(new GetEnvironmentalHealthByIdQuery(TestTenantId.Value, environmentalHealthId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EnvironmentalHealthId.Should().Be(environmentalHealthId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenEnvironmentalHealthDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EnvironmentalHealthItemDto?)null);

        var result = await Sut().Handle(new GetEnvironmentalHealthByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnvironmentalHealth.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EnvironmentalHealth record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEnvironmentalHealthBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EnvironmentalHealthAggregate_GetById filters by
        // TenantId in SQL, so a lookup scoped to TestTenantId for a record that actually
        // belongs to another tenant returns no row at all -- the repository is not stubbed for
        // that (EnvironmentalHealthId, TestTenantId) combination and NSubstitute's default
        // (null) applies, exactly as the real proc would.
        var environmentalHealthId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(environmentalHealthId, otherTenantId, Arg.Any<CancellationToken>()).Returns(EnvironmentalHealthFor(environmentalHealthId, otherTenantId));

        var result = await Sut().Handle(new GetEnvironmentalHealthByIdQuery(TestTenantId.Value, environmentalHealthId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnvironmentalHealth.NotFound");
    }
}
