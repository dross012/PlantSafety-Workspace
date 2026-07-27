// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.AccidentManagement
//  Layer:          Tests (Unit)
//  File:           AccidentAggregateTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Accident aggregate root: full investigation lifecycle
//                  (Create -> ConfirmSiteSecuredAndTriaged -> AssignInvestigator -> RecordRootCause
//                  -> CreateCorrectiveAction -> CompleteCorrectiveAction -> FileRegulatoryReport ->
//                  CloseInvestigation -> Archive), invariant-guard tests for illegal transitions
//                  and the regulatory-filing-required rule, and the AccidentArchivedEvent added in
//                  a prior pass. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.AccidentManagement.Exceptions;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.AccidentManagement;

/// <summary>Unit tests for <see cref="Accident"/> aggregate in the AccidentManagement bounded context.</summary>
public sealed class AccidentAggregateTests : UnitTestBase
{
    private static Accident CreateReported(AccidentSeverity? severity = null) =>
        Accident.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accidentTitle: "Forklift collision, Dock 3",
            location: "Dock 3",
            severity: severity,
            injuredPersonCount: 1,
            ownerId: TestUserId);

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateReported().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusReported()
    {
        var agg = CreateReported();
        agg.Status.Should().Be(AccidentStatus.Reported);
    }

    [Fact]
    public void Create_ShouldRaiseAccidentCreatedEvent()
    {
        var agg = CreateReported();
        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentCreatedEvent>();
    }

    [Fact]
    public void Create_WithLostTimeInjury_ShouldRequireRegulatoryFiling()
    {
        var agg = CreateReported(AccidentSeverity.LostTimeInjury);
        agg.RegulatoryFilingRequired.Should().BeTrue();
    }

    [Fact]
    public void Create_WithFirstAidOnly_ShouldNotRequireRegulatoryFiling()
    {
        var agg = CreateReported(AccidentSeverity.FirstAidOnly);
        agg.RegulatoryFilingRequired.Should().BeFalse();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateReported();
        var a2 = CreateReported();
        a1.AccidentId.Should().NotBe(a2.AccidentId);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- ConfirmSiteSecuredAndTriaged -----------------------------------------

    [Fact]
    public void ConfirmSiteSecuredAndTriaged_WhenReported_ShouldSetFlag()
    {
        var agg = CreateReported();
        agg.ConfirmSiteSecuredAndTriaged();
        agg.SiteSecuredAndTriaged.Should().BeTrue();
    }

    // ---- AssignInvestigator ----------------------------------------------------

    [Fact]
    public void AssignInvestigator_WhenReported_ShouldTransitionToUnderInvestigation()
    {
        var agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccidentStatus.UnderInvestigation);
    }

    [Fact]
    public void AssignInvestigator_WhenReported_ShouldRaiseEvent()
    {
        var agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentInvestigatorAssignedEvent>();
    }

    [Fact]
    public void AssignInvestigator_WhenNotReported_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AccidentInvalidStatusTransitionException>();
    }

    // ---- RecordRootCause -------------------------------------------------------

    private static Accident CreateUnderInvestigation()
    {
        var agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void RecordRootCause_WhenUnderInvestigation_ShouldTransitionToRootCauseIdentified()
    {
        var agg = CreateUnderInvestigation();
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccidentStatus.RootCauseIdentified);
    }

    [Fact]
    public void RecordRootCause_WhenReported_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        var act = () => agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<AccidentInvalidStatusTransitionException>();
    }

    // ---- CorrectiveAction (CAPA) lifecycle --------------------------------------

    private static Accident CreateRootCauseIdentified()
    {
        var agg = CreateUnderInvestigation();
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void CreateCorrectiveAction_WhenRootCauseIdentified_ShouldTransitionToCorrectiveActionsInProgress()
    {
        var agg = CreateRootCauseIdentified();
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccidentStatus.CorrectiveActionsInProgress);
    }

    [Fact]
    public void CreateCorrectiveAction_ShouldAddToCorrectiveActionsCollection()
    {
        var agg = CreateRootCauseIdentified();
        var action = agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CorrectiveActions.Should().ContainSingle(a => a.CorrectiveActionId == action.CorrectiveActionId);
    }

    [Fact]
    public void CreateCorrectiveAction_ShouldRaiseEvent()
    {
        var agg = CreateRootCauseIdentified();
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentCorrectiveActionCreatedEvent>();
    }

    [Fact]
    public void CreateCorrectiveAction_WhenReported_ShouldThrowInvalidOperationException()
    {
        var agg = CreateReported();
        var act = () => agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CompleteCorrectiveAction_WithKnownId_ShouldRaiseEvent()
    {
        var agg = CreateRootCauseIdentified();
        var action = agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.CompleteCorrectiveAction(action.CorrectiveActionId, "Photo evidence", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentCorrectiveActionCompletedEvent>();
    }

    [Fact]
    public void CompleteCorrectiveAction_WithUnknownId_ShouldThrowCorrectiveActionNotFound()
    {
        var agg = CreateRootCauseIdentified();
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.CompleteCorrectiveAction(CorrectiveActionId.New(), "evidence", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CorrectiveActionNotFoundException>();
    }

    // ---- FileRegulatoryReport ---------------------------------------------------

    [Fact]
    public void FileRegulatoryReport_WhenCorrectiveActionsInProgress_ShouldTransitionToRegulatoryFilingComplete()
    {
        var agg = CreateRootCauseIdentified();
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccidentStatus.RegulatoryFilingComplete);
    }

    [Fact]
    public void FileRegulatoryReport_ShouldRaiseEvent()
    {
        var agg = CreateRootCauseIdentified();
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentRegulatoryFilingCompletedEvent>();
    }

    // ---- CloseInvestigation: regulatory-filing-required invariant guard --------

    [Fact]
    public void CloseInvestigation_WhenRegulatoryFilingRequiredButNotFiled_ShouldThrowRegulatoryFilingRequired()
    {
        var agg = CreateReported(AccidentSeverity.LostTimeInjury);
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AccidentRegulatoryFilingRequiredException>();
    }

    [Fact]
    public void CloseInvestigation_WhenRegulatoryFilingRequiredAndFiled_ShouldClose()
    {
        var agg = CreateReported(AccidentSeverity.LostTimeInjury);
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccidentStatus.Closed);
    }

    [Fact]
    public void CloseInvestigation_WhenNotRegulatoryRequired_ShouldCloseWithoutFiling()
    {
        var agg = CreateReported(AccidentSeverity.FirstAidOnly);
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccidentStatus.Closed);
    }

    [Fact]
    public void CloseInvestigation_ShouldRaiseEvent()
    {
        var agg = CreateReported(AccidentSeverity.FirstAidOnly);
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<AccidentInvestigationClosedEvent>();
    }

    // ---- Archive (domain event added in a prior pass) --------------------------

    private static Accident CreateClosed(AccidentSeverity? severity = null)
    {
        var agg = CreateReported(severity ?? AccidentSeverity.FirstAidOnly);
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();
        return agg;
    }

    [Fact]
    public void Archive_WhenClosed_ShouldTransitionToArchived()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccidentStatus.Archived);
    }

    [Fact]
    public void Archive_WhenClosed_ShouldSetArchivedAtUtc()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ArchivedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Archive_WhenClosed_ShouldRaiseAccidentArchivedEvent()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<AccidentArchivedEvent>();
        evt.AccidentId.Should().Be(agg.AccidentId);
    }

    [Fact]
    public void Archive_ArchivedEvent_ShouldHaveSchemaVersion2026()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<AccidentArchivedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Archive_WhenNotClosed_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        var act = () => agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<AccidentInvalidStatusTransitionException>();
    }
}
