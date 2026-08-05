// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Commands
//  Layer:          Tests (Unit)
//  File:           BroadcastPublicAlertCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for BroadcastPublicAlertCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;
using BroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;

namespace PlantSafety.Tests.PublicSafetyCommunication.Commands;

public sealed class BroadcastPublicAlertCommandHandlerTests : UnitTestBase
{
    private readonly IPublicAlertBroadcastRepository _repo = Substitute.For<IPublicAlertBroadcastRepository>();

    private BroadcastPublicAlertCommandHandler Sut() => new(_repo, NullLogger<BroadcastPublicAlertCommandHandler>());

    private static BroadcastPublicAlertCommand ValidCommand(string severityCode = "Warning", string channelCode = "Ipaws") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        SeverityCode: severityCode,
        ChannelCode: channelCode,
        Headline: "Shelter in place",
        Message: "A chemical release has occurred. Residents should shelter in place.",
        TargetArea: "1-mile radius")
    { ActorId = TestUserId.Value };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Sent");
        await _repo.Received(1).AddAsync(Arg.Any<BroadcastAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedSeverity_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand(severityCode: "Catastrophic"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicAlertBroadcast.InvalidSeverity");
        await _repo.DidNotReceive().AddAsync(Arg.Any<BroadcastAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedChannel_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand(channelCode: "CarrierPigeon"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicAlertBroadcast.InvalidChannel");
        await _repo.DidNotReceive().AddAsync(Arg.Any<BroadcastAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<BroadcastAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicAlertBroadcast.BroadcastFailed");
    }
}
