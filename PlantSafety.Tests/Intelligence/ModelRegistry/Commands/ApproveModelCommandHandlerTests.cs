// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.ModelRegistry.Commands
//  Layer:          Tests (Unit)
//  File:           ApproveModelCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL (Safety Rule 4.5) unit tests for ApproveModelCommandHandler.
//                  Confirms the handler always constructs the domain actor as
//                  PerformedByActor.User(...) (never System()/AiAgent()) so the
//                  ModelRegistryEntry aggregate's human-approver invariant is satisfied for every
//                  API-driven approval, plus tenant-scoped NotFound and
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

public sealed class ApproveModelCommandHandlerTests : UnitTestBase
{
    private readonly IModelRegistryEntryRepository _repo = Substitute.For<IModelRegistryEntryRepository>();

    private ApproveModelCommandHandler Sut() => new(_repo, NullLogger<ApproveModelCommandHandler>());

    private static ModelRegistryEntryAggregate PendingApprovalEntry()
    {
        var entry = ModelRegistryEntryAggregate.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "FailurePredictor",
            version: ModelVersion.From("1.0.0"), datasetHash: "sha256:abc", trainingAccuracyScore: 0.85);
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        return entry;
    }

    private static ApproveModelCommand CommandFor(ModelRegistryEntryAggregate entry, string? eTag) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        ModelRegistryEntryId: entry.ModelRegistryEntryId.Value,
        ApprovedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_TransitionsToApproved_SafetyRule45()
    {
        var entry = PendingApprovalEntry();
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);

        var result = await Sut().Handle(CommandFor(entry, "0x01"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Approved");
        entry.ApprovedBy.Should().Be(TestUserId, "Safety Rule 4.5 requires the actor constructed by the handler to be a human user");
        await _repo.Received(1).UpdateAsync(entry, "0x01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ModelRegistryEntryAggregate?)null);

        var result = await Sut().Handle(CommandFor(PendingApprovalEntry(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var entry = PendingApprovalEntry();
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);
        _repo.UpdateAsync(Arg.Any<ModelRegistryEntryAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(entry, "stale"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEntryIsStillDraft_ReturnsGenericFailure()
    {
        var entry = ModelRegistryEntryAggregate.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "FailurePredictor",
            version: ModelVersion.From("1.0.0"), datasetHash: "sha256:abc", trainingAccuracyScore: 0.85);
        _repo.GetByIdAsync(Arg.Any<ModelRegistryEntryId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entry);

        var result = await Sut().Handle(CommandFor(entry, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.ApproveFailed");
    }
}
