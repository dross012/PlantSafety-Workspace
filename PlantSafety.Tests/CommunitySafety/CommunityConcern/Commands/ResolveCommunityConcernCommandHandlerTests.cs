// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Commands
//  Layer:          Tests (Unit)
//  File:           ResolveCommunityConcernCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ResolveCommunityConcernCommandHandler. Covers successful
//                  resolution, not-found handling with correct HTTP-404-mapped severity
//                  (regression coverage for the NotFound-severity defect fixed alongside these
//                  tests), and repository failure handling. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.CommunitySafety.Commands;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Commands;

public sealed class ResolveCommunityConcernCommandHandlerTests : UnitTestBase
{
    private readonly ICommunityConcernRepository _repo = Substitute.For<ICommunityConcernRepository>();

    private ResolveCommunityConcernCommandHandler Sut() => new(_repo, NullLogger<ResolveCommunityConcernCommandHandler>());

    private static Agg OpenConcern() =>
        Agg.Log(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), CommunityConcernCategory.Noise, "Low", "Tank Farm", "desc");

    private static ResolveCommunityConcernCommand CommandFor(Guid communityConcernId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        CommunityConcernId: communityConcernId,
        ResolutionNotes: "Root cause corrected.",
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithExistingConcern_ResolvesAndPersistsAndReturnsSuccess()
    {
        var concern = OpenConcern();
        _repo.GetByIdAsync(concern.CommunityConcernId, Arg.Any<CancellationToken>()).Returns(concern);

        var result = await Sut().Handle(CommandFor(concern.CommunityConcernId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Resolved");
        await _repo.Received(1).UpdateAsync(concern, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConcernDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CommunityConcernId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing CommunityConcern must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        var concern = OpenConcern();
        _repo.GetByIdAsync(concern.CommunityConcernId, Arg.Any<CancellationToken>()).Returns(concern);
        _repo.UpdateAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(CommandFor(concern.CommunityConcernId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.ResolveFailed");
    }
}
