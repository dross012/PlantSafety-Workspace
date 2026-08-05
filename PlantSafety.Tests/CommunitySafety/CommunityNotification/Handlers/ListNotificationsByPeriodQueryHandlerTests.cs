// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityNotification.Handlers
//  Layer:          Tests (Unit)
//  File:           ListNotificationsByPeriodQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListNotificationsByPeriodQueryHandler. Covers successful
//                  retrieval, empty-result handling, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.CommunityNotification.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Models;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Queries;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityNotification.Handlers;

public sealed class ListNotificationsByPeriodQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityNotificationRepository _repo = Substitute.For<ICommunityNotificationRepository>();

    private ListNotificationsByPeriodQueryHandler Sut() => new(_repo, NullLogger<ListNotificationsByPeriodQueryHandler>());

    [Fact]
    public async Task Handle_WithMatchingNotifications_ReturnsSuccessWithItems()
    {
        var item = new CommunityNotificationItemDto
        {
            CommunityNotificationId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            Channel = "Email",
            RecipientContactId = Guid.NewGuid(),
            Subject = "Notice",
            NotificationStatus = "Sent",
            SentAtUtc = FixedUtcNow,
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };
        _repo.GetCommunityNotificationItemsAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityNotificationItemDto> { item });

        var result = await Sut().Handle(new ListNotificationsByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithNoMatchingNotifications_ReturnsSuccessWithEmptyItems()
    {
        _repo.GetCommunityNotificationItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityNotificationItemDto>());

        var result = await Sut().Handle(new ListNotificationsByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCommunityNotificationItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunityNotificationItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListNotificationsByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityNotification.RetrievalFailed");
    }
}
