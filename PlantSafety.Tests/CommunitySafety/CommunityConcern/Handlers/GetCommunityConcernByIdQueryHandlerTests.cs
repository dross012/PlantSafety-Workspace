// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCommunityConcernByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCommunityConcernByIdQueryHandler. Covers successful
//                  retrieval, not-found handling with correct HTTP-404-mapped severity
//                  (regression coverage for the NotFound-severity defect fixed alongside these
//                  tests), tenant isolation, and repository failure handling. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Handlers;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Handlers;

public sealed class GetCommunityConcernByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICommunityConcernRepository _repo = Substitute.For<ICommunityConcernRepository>();

    private GetCommunityConcernByIdQueryHandler Sut() => new(_repo, NullLogger<GetCommunityConcernByIdQueryHandler>());

    private static CommunityConcernItemDto ConcernFor(Guid id, Guid tenantId) => new()
    {
        CommunityConcernId = id,
        TenantId = tenantId,
        SiteId = Guid.NewGuid(),
        Category = "Odor",
        Severity = "Medium",
        FacilityArea = "Tank Farm",
        Description = "desc",
        ConcernStatus = "Open",
        LoggedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenConcernExistsForTenant_ReturnsSuccess()
    {
        var concernId = Guid.NewGuid();
        var concern = ConcernFor(concernId, TestTenantId.Value);
        _repo.GetCommunityConcernItemByIdAsync(TestTenantId.Value, concernId, Arg.Any<CancellationToken>()).Returns(concern);

        var result = await Sut().Handle(new GetCommunityConcernByIdQuery(TestTenantId.Value, concernId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CommunityConcernId.Should().Be(concernId);
    }

    [Fact]
    public async Task Handle_WhenConcernDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetCommunityConcernItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CommunityConcernItemDto?)null);

        var result = await Sut().Handle(new GetCommunityConcernByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing CommunityConcern must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenConcernBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: the repository is scoped by (tenantId, id) at the SQL
        // layer, so a lookup for TestTenantId against a concern that actually belongs to another
        // tenant returns no row -- the repository is not stubbed for that combination.
        var concernId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetCommunityConcernItemByIdAsync(otherTenantId, concernId, Arg.Any<CancellationToken>())
            .Returns(ConcernFor(concernId, otherTenantId));

        var result = await Sut().Handle(new GetCommunityConcernByIdQuery(TestTenantId.Value, concernId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.NotFound");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCommunityConcernItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<CommunityConcernItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetCommunityConcernByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.RetrievalFailed");
    }
}
