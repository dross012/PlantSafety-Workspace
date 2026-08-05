// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityNotification.Commands
//  Layer:          Tests (Unit)
//  File:           SendCommunityNotificationCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SendCommunityNotificationCommandHandler. Covers successful
//                  persistence, invalid Channel rejection, and repository failure handling.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.Commands;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates.CommunityNotification;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Interfaces;

namespace PlantSafety.Tests.CommunitySafety.CommunityNotification.Commands;

public sealed class SendCommunityNotificationCommandHandlerTests : UnitTestBase
{
    private readonly ICommunityNotificationRepository _repo = Substitute.For<ICommunityNotificationRepository>();

    private SendCommunityNotificationCommandHandler Sut() => new(_repo, NullLogger<SendCommunityNotificationCommandHandler>());

    private static SendCommunityNotificationCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: Guid.NewGuid(),
        Channel: "Email",
        RecipientContactId: Guid.NewGuid(),
        Subject: "Planned maintenance notice",
        MessageBody: "Elevated flare activity expected Tuesday 2-4am.",
        RelatedConcernId: null,
        RelatedIncidentId: null,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsNotificationAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Sent");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedChannel_ReturnsFailureWithoutPersisting()
    {
        var command = ValidCommand() with { Channel = "CarrierPigeon" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityNotification.InvalidChannel");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityNotification.SendFailed");
    }
}
