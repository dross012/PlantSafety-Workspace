// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           GetDisposalHistoryByStreamQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for GetDisposalHistoryByStreamQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Handlers;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Models;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class GetDisposalHistoryByStreamQueryHandlerTests : UnitTestBase
{
    private readonly IDisposalCertificateRepository _repo = Substitute.For<IDisposalCertificateRepository>();

    private GetDisposalHistoryByStreamQueryHandler Sut() => new(_repo, NullLogger<GetDisposalHistoryByStreamQueryHandler>());

    private static DisposalCertificateItemDto SampleItem(Guid tenantId, Guid wasteStreamId) => new()
    {
        DisposalCertificateId = Guid.NewGuid(),
        TenantId = tenantId,
        SiteId = null,
        ManifestId = Guid.NewGuid(),
        WasteStreamId = wasteStreamId,
        TsdfFacilityId = Guid.NewGuid(),
        DisposalMethod = "Incineration",
        DisposalDateUtc = FixedUtcNow,
        CertificateReceivedAtUtc = FixedUtcNow,
        Status = "Received",
    };

    [Fact]
    public async Task Handle_WithHistory_ReturnsItems()
    {
        var wasteStreamId = Guid.NewGuid();
        var items = new List<DisposalCertificateItemDto> { SampleItem(TestTenantId.Value, wasteStreamId) };
        _repo.GetDisposalHistoryByStreamAsync(TestTenantId.Value, wasteStreamId, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new GetDisposalHistoryByStreamQuery(TestTenantId.Value, wasteStreamId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithNoHistory_ReturnsEmptyList()
    {
        _repo.GetDisposalHistoryByStreamAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<DisposalCertificateItemDto>());

        var result = await Sut().Handle(new GetDisposalHistoryByStreamQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.GetDisposalHistoryByStreamAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<DisposalCertificateItemDto>>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new GetDisposalHistoryByStreamQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.DisposalCertificate.RetrievalFailed");
    }
}
