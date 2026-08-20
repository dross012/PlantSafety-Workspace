// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.ModelRegistry
//  Layer:          Tests (Unit)
//  File:           ModelRegistryEntryTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ModelRegistryEntry aggregate root: Register factory guards,
//                  the full Draft -> PendingApproval -> Approved -> Deprecated/Retired lifecycle,
//                  invalid-transition guards, the SAFETY-CRITICAL (Safety Rule 4.5) human-approver
//                  invariant on Approve, and domain event emission.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates;
using PlantSafety.Domain.Intelligence.ModelRegistry.Enums;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.Domain.Intelligence.ModelRegistry.Exceptions;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.ModelRegistry;

/// <summary>Unit tests for the <see cref="ModelRegistryEntry"/> aggregate root.</summary>
public sealed class ModelRegistryEntryTests : UnitTestBase
{
    private static ModelRegistryEntry Draft(double accuracy = 0.9, string version = "1.0.0", string modelName = "FailurePredictor") =>
        ModelRegistryEntry.Register(
            registeredBy: TestUserId,
            registeredAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            modelName: modelName,
            version: ModelVersion.From(version),
            datasetHash: "sha256:abc123",
            trainingAccuracyScore: accuracy);

    // ---- Register ------------------------------------------------------------

    [Fact]
    public void Register_WithValidData_StartsInDraftStatus()
    {
        var entry = Draft();
        entry.Status.Should().Be(ModelRegistryEntryStatus.Draft);
    }

    [Fact]
    public void Register_ShouldRaiseModelRegistryEntryRegisteredEvent()
    {
        var entry = Draft();
        entry.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ModelRegistryEntryRegisteredEvent>();
    }

    [Fact]
    public void Register_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => ModelRegistryEntry.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, modelName: "M", version: ModelVersion.From("1.0.0"),
            datasetHash: "h", trainingAccuracyScore: 0.9);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_WithEmptyModelName_ThrowsArgumentException()
    {
        var act = () => ModelRegistryEntry.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "   ", version: ModelVersion.From("1.0.0"),
            datasetHash: "h", trainingAccuracyScore: 0.9);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_WithEmptyDatasetHash_ThrowsArgumentException()
    {
        var act = () => ModelRegistryEntry.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "M", version: ModelVersion.From("1.0.0"),
            datasetHash: "  ", trainingAccuracyScore: 0.9);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Register_WithOutOfRangeAccuracyScore_ThrowsArgumentException(double accuracy)
    {
        var act = () => ModelRegistryEntry.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "M", version: ModelVersion.From("1.0.0"),
            datasetHash: "h", trainingAccuracyScore: accuracy);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_WithNullVersion_ThrowsArgumentNullException()
    {
        var act = () => ModelRegistryEntry.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, modelName: "M", version: null!,
            datasetHash: "h", trainingAccuracyScore: 0.9);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---- SubmitForApproval -----------------------------------------------------

    [Fact]
    public void SubmitForApproval_FromDraft_TransitionsToPendingApproval()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        entry.Status.Should().Be(ModelRegistryEntryStatus.PendingApproval);
    }

    [Fact]
    public void SubmitForApproval_FromNonDraft_ThrowsInvalidStatusTransition()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);

        var act = () => entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ModelRegistryEntryInvalidStatusTransitionException>();
    }

    // ---- Approve (SAFETY-CRITICAL: Safety Rule 4.5) -----------------------------

    [Fact]
    public void Approve_ByHumanActor_FromPendingApproval_TransitionsToApproved()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);

        entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);

        entry.Status.Should().Be(ModelRegistryEntryStatus.Approved);
        entry.ApprovedBy.Should().Be(TestUserId);
        entry.ApprovedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Approve_ByHumanActor_RaisesApprovedEvent()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);

        entry.DomainEvents.Should().Contain(e => e is ModelRegistryEntryApprovedEvent);
    }

    [Fact]
    public void Approve_BySystemActor_ThrowsApprovalRequiresHumanActor_SafetyRule45()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);

        var act = () => entry.Approve(TestUserId, PerformedByActor.System(), FixedTimestamp, TestCorrelationId, TestCausationId);

        act.Should().Throw<ModelRegistryEntryApprovalRequiresHumanActorException>(
            "Safety Rule 4.5 requires an explicit human approver -- a system actor must never approve a model");
        entry.Status.Should().Be(ModelRegistryEntryStatus.PendingApproval, "a rejected approval attempt must not mutate state");
    }

    [Fact]
    public void Approve_ByAiAgentActor_ThrowsApprovalRequiresHumanActor_SafetyRule45()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);

        var act = () => entry.Approve(TestUserId, PerformedByActor.AiAgent(), FixedTimestamp, TestCorrelationId, TestCausationId);

        act.Should().Throw<ModelRegistryEntryApprovalRequiresHumanActorException>(
            "Safety Rule 4.5 requires an explicit human approver -- an AI-agent actor must never approve a model");
    }

    [Fact]
    public void Approve_FromDraft_ThrowsInvalidStatusTransition()
    {
        var entry = Draft();
        var act = () => entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);
        act.Should().Throw<ModelRegistryEntryInvalidStatusTransitionException>();
    }

    // ---- Deprecate ---------------------------------------------------------------

    [Fact]
    public void Deprecate_FromApproved_TransitionsToDeprecated()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);

        entry.Deprecate("Superseded by v2.0.0", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        entry.Status.Should().Be(ModelRegistryEntryStatus.Deprecated);
        entry.DeprecationReason.Should().Be("Superseded by v2.0.0");
        entry.DeprecatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Deprecate_FromDraft_ThrowsInvalidStatusTransition()
    {
        var entry = Draft();
        var act = () => entry.Deprecate("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ModelRegistryEntryInvalidStatusTransitionException>();
    }

    // ---- Retire (terminal) -------------------------------------------------------

    [Fact]
    public void Retire_FromApproved_TransitionsToRetired()
    {
        var entry = Draft();
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);

        entry.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        entry.Status.Should().Be(ModelRegistryEntryStatus.Retired);
        entry.RetiredAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Retire_FromDraft_TransitionsToRetired()
    {
        var entry = Draft();
        entry.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        entry.Status.Should().Be(ModelRegistryEntryStatus.Retired);
    }

    [Fact]
    public void Retire_FromRetired_ThrowsInvalidStatusTransition_TerminalState()
    {
        var entry = Draft();
        entry.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => entry.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ModelRegistryEntryInvalidStatusTransitionException>();
    }

    // ---- Domain events -------------------------------------------------------------

    [Fact]
    public void ClearDomainEvents_EmptiesDomainEventsCollection()
    {
        var entry = Draft();
        entry.DomainEvents.Should().NotBeEmpty();

        entry.ClearDomainEvents();

        entry.DomainEvents.Should().BeEmpty();
    }
}
