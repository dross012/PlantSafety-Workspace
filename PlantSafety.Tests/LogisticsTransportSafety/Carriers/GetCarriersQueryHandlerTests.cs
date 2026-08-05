// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.Carriers
//  Layer:          Tests (Unit)
//  File:           GetCarriersQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCarriersQueryHandler. Read-model repository is mocked with
//                  NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Models;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Queries;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.Carriers;

public sealed class GetCarriersQueryHandlerTests : UnitTestBase
{
    private readonly ICarrierRepository _repo = Substitute.For<ICarrierRepository>();

    private static CarrierItemDto Item() => new()
    {
        CarrierId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        CarrierName = "Acme Freight Lines",
        CarrierType = "ThirdPartyContractCarrier",
        CarrierStatus = "Qualified",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingItems_ShouldReturnSuccessWithItems()
    {
        var items = new List<CarrierItemDto> { Item() };
        _repo.GetCarrierItemsAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);
        var sut = new GetCarriersQueryHandler(_repo, NullLogger<GetCarriersQueryHandler>());

        var result = await sut.Handle(new GetCarriersQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TenantId.Should().Be(TestTenantId.Value);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyItems()
    {
        _repo.GetCarrierItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CarrierItemDto>());
        var sut = new GetCarriersQueryHandler(_repo, NullLogger<GetCarriersQueryHandler>());

        var result = await sut.Handle(new GetCarriersQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.GetCarrierItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CarrierItemDto>>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new GetCarriersQueryHandler(_repo, NullLogger<GetCarriersQueryHandler>());

        var result = await sut.Handle(new GetCarriersQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.RetrievalFailed");
    }
}
