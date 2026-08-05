// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityContact.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCommunityContactsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCommunityContactsQueryHandler. Covers successful retrieval,
//                  empty-result handling, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.CommunityContact.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityContact.Models;
using PlantSafety.Application.CommunitySafety.CommunityContact.Queries;
using PlantSafety.Application.CommunitySafety.CommunityContact.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityContact.Handlers;

public sealed class GetCommunityContactsQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityContactRepository _repo = Substitute.For<ICommunityContactRepository>();

    private GetCommunityContactsQueryHandler Sut() => new(_repo, NullLogger<GetCommunityContactsQueryHandler>());

    private static CommunityContactItemDto ContactFor(Guid id, Guid tenantId) => new()
    {
        CommunityContactId = id,
        TenantId = tenantId,
        DisplayName = "Fire Chief - Station 4",
        ContactType = "EmergencyServices",
        CommunityArea = "North Ridge",
        OptedInForNotifications = true,
        Status = "Active",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingRecords_ReturnsSuccessWithItems()
    {
        var contact = ContactFor(Guid.NewGuid(), TestTenantId.Value);
        _repo.GetCommunityContactItemsAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityContactItemDto> { contact });

        var result = await Sut().Handle(new GetCommunityContactsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle().Which.CommunityContactId.Should().Be(contact.CommunityContactId);
    }

    [Fact]
    public async Task Handle_WithNoMatchingRecords_ReturnsSuccessWithEmptyItems()
    {
        _repo.GetCommunityContactItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityContactItemDto>());

        var result = await Sut().Handle(new GetCommunityContactsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCommunityContactItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunityContactItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetCommunityContactsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityContact.RetrievalFailed");
    }
}
