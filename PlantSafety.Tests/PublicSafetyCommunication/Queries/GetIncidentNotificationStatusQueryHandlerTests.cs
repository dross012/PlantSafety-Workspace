// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Queries
//  Layer:          Tests (Unit)
//  File:           GetIncidentNotificationStatusQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for GetIncidentNotificationStatusQueryHandler. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Handlers;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Models;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Repositories;

namespace PlantSafety.Tests.PublicSafetyCommunication.Queries;

public sealed class GetIncidentNotificationStatusQueryHandlerTests : UnitTestBase
{
    private readonly IIncidentNotificationRepository _repo = Substitute.For<IIncidentNotificationRepository>();

    private GetIncidentNotificationStatusQueryHandler Sut() => new(_repo, NullLogger<GetIncidentNotificationStatusQueryHandler>());

    private static IncidentNotificationItemDto SampleItem(Guid tenantId) => new()
    {
        IncidentNotificationId = Guid.NewGuid(),
        TenantId = tenantId,
        Status = "Dispatched",
        PublicSafetyAgencyId = Guid.NewGuid(),
        ChannelCode = "Phone",
        Message = "Ammonia release -- HazMat requested.",
        CreatedAtUtc = FixedUtcNow,
    };

    [Fact]
    public async Task Handle_ReturnsItemsFromRepositoryForDefaultDateRange()
    {
        var from = FixedUtcNow.AddDays(-90);
        var to = FixedUtcNow.AddDays(90);
        var items = new[] { SampleItem(TestTenantId.Value) };
        _repo.GetIncidentNotificationStatusAsync(TestTenantId.Value, null, null, from, to, Arg.Any<CancellationToken>())
            .Returns(items);

        var query = new GetIncidentNotificationStatusQuery(TestTenantId.Value, null, null, from, to);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetIncidentNotificationStatusAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<IncidentNotificationItemDto>>>(_ => throw new InvalidOperationException("boom"));

        var query = new GetIncidentNotificationStatusQuery(TestTenantId.Value, null, null, FixedUtcNow.AddDays(-1), FixedUtcNow);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.StatusRetrievalFailed");
    }
}
