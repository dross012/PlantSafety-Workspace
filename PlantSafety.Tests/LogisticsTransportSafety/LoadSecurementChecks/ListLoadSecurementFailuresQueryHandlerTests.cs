// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Tests (Unit)
//  File:           ListLoadSecurementFailuresQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListLoadSecurementFailuresQueryHandler. Read-model repository
//                  is mocked with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Models;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Queries;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.LoadSecurementChecks;

public sealed class ListLoadSecurementFailuresQueryHandlerTests : UnitTestBase
{
    private readonly ILoadSecurementCheckRepository _repo = Substitute.For<ILoadSecurementCheckRepository>();

    private static LoadSecurementCheckItemDto Item() => new()
    {
        LoadSecurementCheckId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        VehicleId = Guid.NewGuid(),
        CheckStatus = "Failed",
        DeficiencyCount = 3,
        SecurementScore = 55m,
        CheckedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingItems_ShouldReturnSuccessWithItems()
    {
        _repo.ListLoadSecurementFailuresAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<LoadSecurementCheckItemDto> { Item() });
        var sut = new ListLoadSecurementFailuresQueryHandler(_repo, NullLogger<ListLoadSecurementFailuresQueryHandler>());

        var result = await sut.Handle(new ListLoadSecurementFailuresQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].CheckStatus.Should().Be("Failed");
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyItems()
    {
        _repo.ListLoadSecurementFailuresAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<LoadSecurementCheckItemDto>());
        var sut = new ListLoadSecurementFailuresQueryHandler(_repo, NullLogger<ListLoadSecurementFailuresQueryHandler>());

        var result = await sut.Handle(new ListLoadSecurementFailuresQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.ListLoadSecurementFailuresAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<LoadSecurementCheckItemDto>>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new ListLoadSecurementFailuresQueryHandler(_repo, NullLogger<ListLoadSecurementFailuresQueryHandler>());

        var result = await sut.Handle(new ListLoadSecurementFailuresQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.LoadSecurementChecks.RetrievalFailed");
    }
}
