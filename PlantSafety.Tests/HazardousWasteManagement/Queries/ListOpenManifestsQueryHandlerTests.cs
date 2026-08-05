// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           ListOpenManifestsQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for ListOpenManifestsQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Manifests.Handlers;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class ListOpenManifestsQueryHandlerTests : UnitTestBase
{
    private readonly IHazardousWasteManifestRepository _repo = Substitute.For<IHazardousWasteManifestRepository>();

    private ListOpenManifestsQueryHandler Sut() => new(_repo, NullLogger<ListOpenManifestsQueryHandler>());

    private static HazardousWasteManifestItemDto SampleItem(Guid tenantId) => new()
    {
        HazardousWasteManifestId = Guid.NewGuid(),
        TenantId = tenantId,
        SiteId = null,
        ManifestNumber = "MAN-2026-000123",
        GeneratorEpaId = "EPA123456789",
        WasteStreamId = Guid.NewGuid(),
        TransporterId = null,
        TsdfFacilityId = null,
        Status = "Generated",
        GeneratedAtUtc = FixedUtcNow,
        TransporterSignedAtUtc = null,
        TsdfReceivedAtUtc = null,
        ClosedAtUtc = null,
        IsImmutable = false,
        DiscrepancyReason = null,
        RetentionExpiresAtUtc = FixedUtcNow.AddYears(3),
    };

    [Fact]
    public async Task Handle_WithOpenManifests_ReturnsItems()
    {
        var items = new List<HazardousWasteManifestItemDto> { SampleItem(TestTenantId.Value), SampleItem(TestTenantId.Value) };
        _repo.ListOpenManifestsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListOpenManifestsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoOpenManifests_ReturnsEmptyList()
    {
        _repo.ListOpenManifestsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new List<HazardousWasteManifestItemDto>());

        var result = await Sut().Handle(new ListOpenManifestsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.ListOpenManifestsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<HazardousWasteManifestItemDto>>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new ListOpenManifestsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.ListOpenFailed");
    }
}
