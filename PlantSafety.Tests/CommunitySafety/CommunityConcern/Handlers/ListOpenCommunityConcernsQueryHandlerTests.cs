// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Handlers
//  Layer:          Tests (Unit)
//  File:           ListOpenCommunityConcernsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListOpenCommunityConcernsQueryHandler. Covers successful
//                  retrieval, empty-result handling, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.CommunityConcern.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Handlers;

public sealed class ListOpenCommunityConcernsQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityConcernRepository _repo = Substitute.For<ICommunityConcernRepository>();

    private ListOpenCommunityConcernsQueryHandler Sut() => new(_repo, NullLogger<ListOpenCommunityConcernsQueryHandler>());

    [Fact]
    public async Task Handle_WithOpenConcerns_ReturnsSuccessWithItems()
    {
        var item = new CommunityConcernItemDto
        {
            CommunityConcernId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            SiteId = Guid.NewGuid(),
            Category = "Noise",
            Severity = "Low",
            FacilityArea = "Tank Farm",
            Description = "desc",
            ConcernStatus = "Open",
            LoggedAtUtc = FixedUtcNow,
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };
        _repo.GetOpenCommunityConcernItemsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>())
            .Returns(new List<CommunityConcernItemDto> { item });

        var result = await Sut().Handle(new ListOpenCommunityConcernsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithNoOpenConcerns_ReturnsSuccessWithEmptyItems()
    {
        _repo.GetOpenCommunityConcernItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommunityConcernItemDto>());

        var result = await Sut().Handle(new ListOpenCommunityConcernsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetOpenCommunityConcernItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunityConcernItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListOpenCommunityConcernsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.RetrievalFailed");
    }
}
