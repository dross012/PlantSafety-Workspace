// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityMeetingRecord.Commands
//  Layer:          Tests (Unit)
//  File:           RecordCommunityMeetingCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordCommunityMeetingCommandHandler. Covers successful
//                  persistence and repository failure handling. Isolated, deterministic, <50ms
//                  per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.Commands;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates.CommunityMeetingRecord;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Interfaces;

namespace PlantSafety.Tests.CommunitySafety.CommunityMeetingRecord.Commands;

public sealed class RecordCommunityMeetingCommandHandlerTests : UnitTestBase
{
    private readonly ICommunityMeetingRecordRepository _repo = Substitute.For<ICommunityMeetingRecordRepository>();

    private RecordCommunityMeetingCommandHandler Sut() => new(_repo, NullLogger<RecordCommunityMeetingCommandHandler>());

    private static RecordCommunityMeetingCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: Guid.NewGuid(),
        MeetingTitle: "Q3 Community Advisory Panel",
        MeetingDateUtc: FixedUtcNow,
        Location: "Community Center, Room B",
        AttendeeCount: 12,
        TopicsSummary: "Reviewed Q3 flare activity and upcoming maintenance schedule.",
        ActionItems: "Publish maintenance schedule to community newsletter.",
        RecordedByUserId: TestUserId.Value,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsMeetingRecordAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Recorded");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityMeetingRecord.RecordFailed");
    }
}
