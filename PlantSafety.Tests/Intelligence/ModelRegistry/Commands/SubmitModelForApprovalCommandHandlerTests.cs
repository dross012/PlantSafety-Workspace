// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.ModelRegistry.Commands
//  Layer:          Tests (Unit)
//  File:           SubmitModelForApprovalCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SubmitModelForApprovalCommandHandler. Covers Draft ->
//                  PendingApproval transition, caller ETag forwarding, tenant-scoped NotFound, and
//                  ConcurrencyConflictException propagation. Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Commands.StartProcessingDataLakeActivityCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.ModelRegistry.Commands;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;
using ModelRegistryEntryAggregate = PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry;

namespace PlantSafety.Tests.Intelligence.ModelRegistry.Commands;

public sealed class SubmitModelForApprovalCommandHandlerTests : UnitTestBase
{
    private readonly IModelRegistryEntryRepository _repo = Substitute.For<IModelRegistryEntryRepository>();

    private SubmitModelForApprovalCommandHandler Sut() => new(_repo, NullLogger<SubmitModelForApprovalCommandHandler>());

    private static ModelRegistryEntryAggregate DraftEntry() =>
        ModelRegistryEntryAggregate.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "FailurePredictor",
            version: ModelVersion.From("1.0.0"), datasetHash: "sha256:abc", trainingAccuracyScore: 0.85);

    private static SubmitModelForApprovalCommand CommandFor(ModelRegistryEntryAggregate entry, string? eTag) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        ModelRegistryEntryId: entry.ModelRegistryEntryId.Value,
        SubmittedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_TransitionsToPendingApprovalAndForwardsEtag()
    {
        var entry = DraftEntry();
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);

        var result = await Sut().Handle(CommandFor(entry, "0x01"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("PendingApproval");
        await _repo.Received(1).UpdateAsync(entry, "0x01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ModelRegistryEntryAggregate?)null);

        var result = await Sut().Handle(CommandFor(DraftEntry(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var entry = DraftEntry();
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);
        _repo.UpdateAsync(Arg.Any<ModelRegistryEntryAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(entry, "stale"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEntryAlreadyPendingApproval_ReturnsGenericFailure()
    {
        var entry = DraftEntry();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);

        var result = await Sut().Handle(CommandFor(entry, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.SubmitForApprovalFailed");
    }
}
