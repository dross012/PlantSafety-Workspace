// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.EnvironmentalMonitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEnvironmentalMonitoringQueryHandlerTests.cs
//  Created:        2026-08-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEnvironmentalMonitoringQueryHandler -- covers successful retrieval
//                  (tenant-wide and site-scoped), repository failure mapping to a sanitized
//                  ApplicationError, and the empty-result-set case. Mirrors the
//                  PlantSafety.Tests.AccidentManagement.Handlers.GetAccidentByIdQueryHandlerTests
//                  shape, adapted for a list-returning read-model query.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Handlers;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Queries;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Repositories;

namespace PlantSafety.Tests.Operational.EnvironmentalMonitoring.Handlers;

public sealed class GetEnvironmentalMonitoringQueryHandlerTests : UnitTestBase
{
    private readonly IEnvironmentalMonitoringRepository _repo = Substitute.For<IEnvironmentalMonitoringRepository>();

    private GetEnvironmentalMonitoringQueryHandler Sut() => new(_repo, NullLogger<GetEnvironmentalMonitoringQueryHandler>());

    private static EnvironmentalMonitoringItemDto ItemFor(Guid recordId, Guid tenantId, Guid? siteId = null) => new()
    {
        EnvironmentalMonitoringRecordId = recordId,
        TenantId = tenantId,
        SiteId = siteId,
        SensorId = Guid.NewGuid(),
        SensorName = "Zone 3 Air Quality Sensor",
        ReadingType = "AirQuality",
        ReadingValue = 42.5m,
        Status = "Normal",
        RecordedAtUtc = FixedUtcNow,
        RecordedBy = null,
    };

    [Fact]
    public async Task Handle_WhenRepositoryReturnsItems_ReturnsSuccessWithTenantAndItems()
    {
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value);
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;

        _repo.GetEnvironmentalMonitoringAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<EnvironmentalMonitoringItemDto> { item });

        var result = await Sut().Handle(new GetEnvironmentalMonitoringQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TestTenantId.Value);
        result.Value.SiteId.Should().BeNull();
        result.Value.Items.Should().ContainSingle().Which.EnvironmentalMonitoringRecordId.Should().Be(recordId);
    }

    [Fact]
    public async Task Handle_WhenSiteScoped_PassesSiteIdThroughToRepositoryAndResult()
    {
        var siteId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value, siteId);
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetEnvironmentalMonitoringAsync(TestTenantId.Value, siteId, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<EnvironmentalMonitoringItemDto> { item });

        var result = await Sut().Handle(new GetEnvironmentalMonitoringQuery(TestTenantId.Value, siteId, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SiteId.Should().Be(siteId);
        result.Value.Items.Should().ContainSingle().Which.SiteId.Should().Be(siteId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNoRows_ReturnsSuccessWithEmptyItems()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetEnvironmentalMonitoringAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<EnvironmentalMonitoringItemDto>());

        var result = await Sut().Handle(new GetEnvironmentalMonitoringQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsSanitizedFailureResult()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetEnvironmentalMonitoringAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<EnvironmentalMonitoringItemDto>>>(_ => throw new InvalidOperationException("simulated SQL timeout"));

        var result = await Sut().Handle(new GetEnvironmentalMonitoringQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.EnvironmentalMonitoring.RetrievalFailed");
        result.Error.Message.Should().NotContain("simulated SQL timeout", "the error message must never leak raw exception detail");
    }
}