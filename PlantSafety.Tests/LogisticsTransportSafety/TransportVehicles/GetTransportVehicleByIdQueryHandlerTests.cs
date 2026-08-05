// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportVehicles
//  Layer:          Tests (Unit)
//  File:           GetTransportVehicleByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetTransportVehicleByIdQueryHandler. Read-model repository is
//                  mocked with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Models;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportVehicles;

public sealed class GetTransportVehicleByIdQueryHandlerTests : UnitTestBase
{
    private readonly ITransportVehicleRepository _repo = Substitute.For<ITransportVehicleRepository>();

    private static TransportVehicleItemDto Item(Guid id) => new()
    {
        TransportVehicleId = id,
        TenantId = TestTenantId.Value,
        VehicleTitle = "Forklift 12",
        VehicleType = "Forklift",
        VehicleIdentifier = "FLT-012",
        VehicleStatus = "Registered",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenFound_ShouldReturnSuccessWithItem()
    {
        var vehicleId = Guid.NewGuid();
        _repo.GetTransportVehicleByIdAsync(TestTenantId.Value, vehicleId, Arg.Any<CancellationToken>())
            .Returns(Item(vehicleId));
        var sut = new GetTransportVehicleByIdQueryHandler(_repo, NullLogger<GetTransportVehicleByIdQueryHandler>());

        var result = await sut.Handle(new GetTransportVehicleByIdQuery(TestTenantId.Value, vehicleId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().NotBeNull();
        result.Value.Item!.TransportVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldReturnSuccessWithNullItem()
    {
        _repo.GetTransportVehicleByIdAsync(TestTenantId.Value, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((TransportVehicleItemDto?)null);
        var sut = new GetTransportVehicleByIdQueryHandler(_repo, NullLogger<GetTransportVehicleByIdQueryHandler>());

        var result = await sut.Handle(new GetTransportVehicleByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.GetTransportVehicleByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<TransportVehicleItemDto?>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new GetTransportVehicleByIdQueryHandler(_repo, NullLogger<GetTransportVehicleByIdQueryHandler>());

        var result = await sut.Handle(new GetTransportVehicleByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportVehicles.RetrievalFailed");
    }
}
