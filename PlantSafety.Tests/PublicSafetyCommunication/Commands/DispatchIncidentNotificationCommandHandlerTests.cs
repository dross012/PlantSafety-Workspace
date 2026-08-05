// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Commands
//  Layer:          Tests (Unit)
//  File:           DispatchIncidentNotificationCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for DispatchIncidentNotificationCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using IncidentNotificationAggregate = PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification;

namespace PlantSafety.Tests.PublicSafetyCommunication.Commands;

public sealed class DispatchIncidentNotificationCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentNotificationRepository _repo = Substitute.For<IIncidentNotificationRepository>();

    private DispatchIncidentNotificationCommandHandler Sut() => new(_repo, NullLogger<DispatchIncidentNotificationCommandHandler>());

    private static DispatchIncidentNotificationCommand ValidCommand(string channelCode = "Phone") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        PublicSafetyAgencyId: Guid.NewGuid(),
        DeliveryChannelCode: channelCode,
        Message: "Ammonia release detected -- requesting HazMat response.")
    { ActorId = TestUserId.Value };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Dispatched");
        await _repo.Received(1).AddAsync(Arg.Any<IncidentNotificationAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedDeliveryChannel_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand("NotARealChannel"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.InvalidChannel");
        await _repo.DidNotReceive().AddAsync(Arg.Any<IncidentNotificationAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<IncidentNotificationAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.DispatchFailed");
    }
}
