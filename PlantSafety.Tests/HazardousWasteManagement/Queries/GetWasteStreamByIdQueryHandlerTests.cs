// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           GetWasteStreamByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for GetWasteStreamByIdQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Handlers;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Models;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class GetWasteStreamByIdQueryHandlerTests : UnitTestBase
{
    private readonly IWasteStreamRepository _repo = Substitute.For<IWasteStreamRepository>();

    private GetWasteStreamByIdQueryHandler Sut() => new(_repo, NullLogger<GetWasteStreamByIdQueryHandler>());

    private static WasteStreamItemDto SampleItem(Guid tenantId, Guid wasteStreamId) => new()
    {
        WasteStreamId = wasteStreamId,
        TenantId = tenantId,
        SiteId = null,
        StreamName = "Waste Oil Stream",
        EpaWasteCode = "D001",
        HazardClass = "Ignitable",
        GeneratorEpaId = "EPA123456789",
        CharacterizationVersion = 1,
        Status = "Characterized",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenFound_ReturnsItem()
    {
        var wasteStreamId = Guid.NewGuid();
        var item = SampleItem(TestTenantId.Value, wasteStreamId);
        _repo.GetWasteStreamByIdAsync(TestTenantId.Value, wasteStreamId, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new GetWasteStreamByIdQuery(TestTenantId.Value, wasteStreamId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsSuccessWithNullItem()
    {
        _repo.GetWasteStreamByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WasteStreamItemDto?)null);

        var result = await Sut().Handle(new GetWasteStreamByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.GetWasteStreamByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<WasteStreamItemDto?>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new GetWasteStreamByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteStream.RetrievalFailed");
    }
}
