// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityMeetingRecord.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCommunityMeetingRecordsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCommunityMeetingRecordsQueryHandler. Covers successful
//                  retrieval, empty-result handling, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Models;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Queries;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityMeetingRecord.Handlers;

public sealed class GetCommunityMeetingRecordsQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityMeetingRecordRepository _repo = Substitute.For<ICommunityMeetingRecordRepository>();

    private GetCommunityMeetingRecordsQueryHandler Sut() => new(_repo, NullLogger<GetCommunityMeetingRecordsQueryHandler>());

    [Fact]
    public async Task Handle_WithMatchingRecords_ReturnsSuccessWithItems()
    {
        var item = new CommunityMeetingRecordItemDto
        {
            CommunityMeetingRecordId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            SiteId = Guid.NewGuid(),
            MeetingTitle = "Q3 Community Advisory Panel",
            MeetingDateUtc = FixedUtcNow,
            Location = "Community Center",
            AttendeeCount = 12,
            TopicsSummary = "topics",
            RecordStatus = "Recorded",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };
        _repo.GetCommunityMeetingRecordItemsAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityMeetingRecordItemDto> { item });

        var result = await Sut().Handle(new GetCommunityMeetingRecordsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithNoMatchingRecords_ReturnsSuccessWithEmptyItems()
    {
        _repo.GetCommunityMeetingRecordItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityMeetingRecordItemDto>());

        var result = await Sut().Handle(new GetCommunityMeetingRecordsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCommunityMeetingRecordItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunityMeetingRecordItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetCommunityMeetingRecordsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityMeetingRecord.RetrievalFailed");
    }
}
