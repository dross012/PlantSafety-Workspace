// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Queries
//  Layer:          Tests (Unit)
//  File:           ListActiveBroadcastsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for ListActiveBroadcastsQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Handlers;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Models;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Repositories;

namespace PlantSafety.Tests.PublicSafetyCommunication.Queries;

public sealed class ListActiveBroadcastsQueryHandlerTests : UnitTestBase
{
    private readonly IPublicAlertBroadcastRepository _repo = Substitute.For<IPublicAlertBroadcastRepository>();

    private ListActiveBroadcastsQueryHandler Sut() => new(_repo, NullLogger<ListActiveBroadcastsQueryHandler>());

    private static PublicAlertBroadcastItemDto SampleItem(Guid tenantId) => new()
    {
        PublicAlertBroadcastId = Guid.NewGuid(),
        TenantId = tenantId,
        Status = "Sent",
        SeverityCode = "Warning",
        ChannelCode = "Ipaws",
        Headline = "Shelter in place",
        Message = "A chemical release has occurred.",
        TargetArea = "1-mile radius",
        CreatedAtUtc = FixedUtcNow,
    };

    [Fact]
    public async Task Handle_ReturnsItemsFromRepositoryScopedToTenant()
    {
        var items = new[] { SampleItem(TestTenantId.Value) };
        _repo.ListActiveBroadcastsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListActiveBroadcastsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.ListActiveBroadcastsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<PublicAlertBroadcastItemDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(new ListActiveBroadcastsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicAlertBroadcast.ListActiveFailed");
    }
}
