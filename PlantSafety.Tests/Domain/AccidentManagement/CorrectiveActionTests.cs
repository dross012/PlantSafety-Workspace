// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.AccidentManagement
//  Layer:          Tests (Unit)
//  File:           CorrectiveActionTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CorrectiveAction child entity: creation defaults and the
//                  Start/Complete/VerifyEffective/MarkOverdue invariant guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Entities;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Tests.Domain.AccidentManagement;

/// <summary>Unit tests for <see cref="CorrectiveAction"/>.</summary>
public sealed class CorrectiveActionTests : UnitTestBase
{
    private static CorrectiveAction CreateOpen() =>
        new(AccidentId.New(), CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp);

    [Fact]
    public void Create_ShouldSetStatusOpen()
    {
        CreateOpen().Status.Should().Be(CorrectiveActionStatus.Open);
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        CreateOpen().CorrectiveActionId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Start_WhenOpen_ShouldTransitionToInProgress()
    {
        var action = CreateOpen();
        action.Start();
        action.Status.Should().Be(CorrectiveActionStatus.InProgress);
    }

    [Fact]
    public void Start_WhenNotOpen_ShouldThrowInvalidOperationException()
    {
        var action = CreateOpen();
        action.Start();
        var act = () => action.Start();
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(CorrectiveActionStatus.Open)]
    [InlineData(CorrectiveActionStatus.InProgress)]
    public void Complete_FromOpenOrInProgress_ShouldTransitionToCompleted(CorrectiveActionStatus fromStatus)
    {
        var action = CreateOpen();
        if (fromStatus == CorrectiveActionStatus.InProgress)
            action.Start();

        action.Complete("Evidence photo", FixedTimestamp);

        action.Status.Should().Be(CorrectiveActionStatus.Completed);
    }

    [Fact]
    public void Complete_FromOverdue_ShouldTransitionToCompleted()
    {
        var action = CreateOpen();
        action.MarkOverdue();

        action.Complete("Evidence photo", FixedTimestamp);

        action.Status.Should().Be(CorrectiveActionStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldSetCompletionEvidenceAndTimestamp()
    {
        var action = CreateOpen();
        action.Complete("Evidence photo", FixedTimestamp);
        action.CompletionEvidence.Should().Be("Evidence photo");
        action.CompletedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        var action = CreateOpen();
        action.Complete("Evidence photo", FixedTimestamp);
        var act = () => action.Complete("Second evidence", FixedTimestamp);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void VerifyEffective_WhenCompleted_ShouldTransitionToVerifiedEffective()
    {
        var action = CreateOpen();
        action.Complete("Evidence photo", FixedTimestamp);

        action.VerifyEffective(TestUserId, "Follow-up inspection", FixedTimestamp);

        action.Status.Should().Be(CorrectiveActionStatus.VerifiedEffective);
    }

    [Fact]
    public void VerifyEffective_ShouldSetVerifiedByAndSteps()
    {
        var action = CreateOpen();
        action.Complete("Evidence photo", FixedTimestamp);

        action.VerifyEffective(TestUserId, "Follow-up inspection", FixedTimestamp);

        action.VerifiedBy.Should().Be(TestUserId);
        action.VerificationSteps.Should().Be("Follow-up inspection");
    }

    [Fact]
    public void VerifyEffective_WhenNotCompleted_ShouldThrowInvalidOperationException()
    {
        var action = CreateOpen();
        var act = () => action.VerifyEffective(TestUserId, "Follow-up inspection", FixedTimestamp);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(CorrectiveActionStatus.Open)]
    [InlineData(CorrectiveActionStatus.InProgress)]
    public void MarkOverdue_FromOpenOrInProgress_ShouldTransitionToOverdue(CorrectiveActionStatus fromStatus)
    {
        var action = CreateOpen();
        if (fromStatus == CorrectiveActionStatus.InProgress)
            action.Start();

        action.MarkOverdue();

        action.Status.Should().Be(CorrectiveActionStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_WhenCompleted_ShouldNotChangeStatus()
    {
        var action = CreateOpen();
        action.Complete("Evidence photo", FixedTimestamp);

        action.MarkOverdue();

        action.Status.Should().Be(CorrectiveActionStatus.Completed);
    }
}
