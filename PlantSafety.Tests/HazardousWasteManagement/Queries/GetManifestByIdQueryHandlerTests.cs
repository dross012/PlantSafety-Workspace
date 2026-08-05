// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           GetManifestByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for GetManifestByIdQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Manifests.Handlers;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class GetManifestByIdQueryHandlerTests : UnitTestBase
{
    private readonly IHazardousWasteManifestRepository _repo = Substitute.For<IHazardousWasteManifestRepository>();

    private GetManifestByIdQueryHandler Sut() => new(_repo, NullLogger<GetManifestByIdQueryHandler>());

    private static HazardousWasteManifestItemDto SampleItem(Guid tenantId, Guid manifestId) => new()
    {
        HazardousWasteManifestId = manifestId,
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
    public async Task Handle_WhenFound_ReturnsItem()
    {
        var manifestId = Guid.NewGuid();
        var item = SampleItem(TestTenantId.Value, manifestId);
        _repo.GetManifestByIdAsync(TestTenantId.Value, manifestId, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new GetManifestByIdQuery(TestTenantId.Value, manifestId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsSuccessWithNullItem()
    {
        _repo.GetManifestByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HazardousWasteManifestItemDto?)null);

        var result = await Sut().Handle(new GetManifestByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.GetManifestByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<HazardousWasteManifestItemDto?>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new GetManifestByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.RetrievalFailed");
    }
}
