// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ContinuousImprovement.CAPA
//  Layer:          Tests (Unit)
//  File:           CorrectiveActionAggregateTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CAPA CorrectiveAction aggregate root, replacing the
//                  CAPADomainStubTests placeholder now that the module has a real aggregate.
//                  Covers Create invariants, the Open -> InProgress -> AwaitingVerification ->
//                  Closed lifecycle, the Overdue side-transition, illegal-transition guards, and
//                  domain event emission. Isolated, deterministic, &lt;50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Enums;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Events;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.ContinuousImprovement.CAPA;

/// <summary>Unit tests for <see cref="CorrectiveAction"/> (CAPA aggregate root).</summary>
public sealed class CorrectiveActionAggregateTests : UnitTestBase
{
    private static CorrectiveAction CreateOpen() =>
        CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            sourceEventId: Guid.NewGuid(),
            sourceEventType: "Incident",
            title: "Repair guard rail",
            description: "Guard rail bent in forklift collision",
            actionType: "Corrective",
            priority: "High",
            assignedToWorkerId: TestUserId,
            dueAtUtc: FixedTimestamp);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateOpen().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusOpen()
    {
        var agg = CreateOpen();
        agg.Status.Should().Be(CorrectiveActionStatus.Open);
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = CreateOpen();
        agg.CorrectiveActionId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldRaiseCorrectiveActionCreatedEvent()
    {
        var agg = CreateOpen();
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<CorrectiveActionCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateOpen();
        var a2 = CreateOpen();
        a1.CorrectiveActionId.Should().NotBe(a2.CorrectiveActionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException(string title)
    {
        var act = () => CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, sourceEventId: Guid.NewGuid(), sourceEventType: "Incident",
            title: title, description: "desc", actionType: "Corrective", priority: "High",
            assignedToWorkerId: TestUserId, dueAtUtc: FixedTimestamp);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptySourceEventId_ShouldThrowArgumentException()
    {
        var act = () => CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, sourceEventId: Guid.Empty, sourceEventType: "Incident",
            title: "Title", description: "desc", actionType: "Corrective", priority: "High",
            assignedToWorkerId: TestUserId, dueAtUtc: FixedTimestamp);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAssignedToWorkerId_ShouldThrowArgumentException()
    {
        var act = () => CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, sourceEventId: Guid.NewGuid(), sourceEventType: "Incident",
            title: "Title", description: "desc", actionType: "Corrective", priority: "High",
            assignedToWorkerId: default, dueAtUtc: FixedTimestamp);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = CreateOpen();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- Start -------------------------------------------------------------------

    [Fact]
    public void Start_WhenOpen_ShouldTransitionToInProgress()
    {
        var agg = CreateOpen();
        agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CorrectiveActionStatus.InProgress);
    }

    [Fact]
    public void Start_ShouldRaiseCorrectiveActionStartedEvent()
    {
        var agg = CreateOpen();
        agg.ClearDomainEvents();
        agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CorrectiveActionStartedEvent>();
    }

    [Fact]
    public void Start_WhenNotOpen_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateOpen();
        agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CorrectiveActionInvalidStatusTransitionException>();
    }

    // ---- Complete ------------------------------------------------------------------

    [Theory]
    [InlineData(CorrectiveActionStatus.Open)]
    [InlineData(CorrectiveActionStatus.InProgress)]
    [InlineData(CorrectiveActionStatus.Overdue)]
    public void Complete_FromOpenInProgressOrOverdue_ShouldTransitionToAwaitingVerification(CorrectiveActionStatus fromStatus)
    {
        var agg = CreateOpen();
        if (fromStatus == CorrectiveActionStatus.InProgress)
            agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        else if (fromStatus == CorrectiveActionStatus.Overdue)
            agg.MarkOverdue(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CorrectiveActionStatus.AwaitingVerification);
    }

    [Fact]
    public void Complete_ShouldSetCompletedAtUtc()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CompletedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Complete_ShouldRaiseCorrectiveActionCompletedEvent()
    {
        var agg = CreateOpen();
        agg.ClearDomainEvents();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CorrectiveActionCompletedEvent>();
    }

    [Fact]
    public void Complete_WhenAlreadyAwaitingVerification_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CorrectiveActionInvalidStatusTransitionException>();
    }

    // ---- Close ---------------------------------------------------------------------

    [Fact]
    public void Close_WhenAwaitingVerification_ShouldTransitionToClosed()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CorrectiveActionStatus.Closed);
    }

    [Fact]
    public void Close_ShouldRaiseCorrectiveActionClosedEvent()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<CorrectiveActionClosedEvent>();
    }

    [Fact]
    public void Close_WhenNotAwaitingVerification_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateOpen();

        var act = () => agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CorrectiveActionInvalidStatusTransitionException>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CorrectiveActionInvalidStatusTransitionException>();
    }

    // ---- MarkOverdue -----------------------------------------------------------------

    [Theory]
    [InlineData(CorrectiveActionStatus.Open)]
    [InlineData(CorrectiveActionStatus.InProgress)]
    public void MarkOverdue_FromOpenOrInProgress_ShouldTransitionToOverdue(CorrectiveActionStatus fromStatus)
    {
        var agg = CreateOpen();
        if (fromStatus == CorrectiveActionStatus.InProgress)
            agg.Start(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.MarkOverdue(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CorrectiveActionStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_ShouldRaiseCorrectiveActionMarkedOverdueEvent()
    {
        var agg = CreateOpen();
        agg.ClearDomainEvents();
        agg.MarkOverdue(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CorrectiveActionMarkedOverdueEvent>();
    }

    [Fact]
    public void MarkOverdue_WhenAwaitingVerification_ShouldBeNoOp()
    {
        var agg = CreateOpen();
        agg.Complete(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.MarkOverdue(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CorrectiveActionStatus.AwaitingVerification);
        agg.DomainEvents.Should().BeEmpty();
    }
}
