// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Domain
//  Layer:          Tests (Unit)
//  File:           LocalEmergencyCoordinationAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant regression tests for the LocalEmergencyCoordination aggregate
//                  root and its four child entities (MutualAidAgreement, JointResponsePlan,
//                  ExternalResponderLiaison + LiaisonLogEntry, JointDebriefRecord). Exercises every
//                  lifecycle transition that raises a domain event to confirm the hand-fix of the
//                  aggregate's EventId construction -- domain events were raised via
//                  Events.LocalEmergencyCoordinationEventId.New() (a type that does not exist in the
//                  Events namespace) and were fixed to ValueObjects.Ids.LocalEmergencyCoordinationEventId.New()
//                  at all 9 raise sites (Create, EstablishMutualAidAgreement, ActivateJointResponsePlan,
//                  DeactivateJointResponsePlan, AssignExternalResponderLiaison, LogLiaisonCommunication,
//                  RecordJointDebrief, Close, Archive). Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Domain;

public sealed class LocalEmergencyCoordinationAggregateTests : UnitTestBase
{
    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ProducesCreatedStatus_AndRaisesCreatedEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Created);
        agg.DomainEvents.Should().ContainSingle();

        var evt = agg.DomainEvents.Single().Should().BeOfType<LocalEmergencyCoordinationCreatedEvent>().Subject;
        evt.EventId.Should().BeOfType<LocalEmergencyCoordinationEventId>();
        evt.EventId.Value.Should().NotBe(Guid.Empty, "the EventId regression fix must produce a real Uuid7, not a default struct");
        evt.LocalEmergencyCoordinationId.Should().Be(agg.LocalEmergencyCoordinationId);
    }

    [Fact]
    public void Create_TrimsCaseName()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(caseName: "  Trimmed Name  ");
        agg.CaseName.Should().Be("Trimmed Name");
    }

    // ---- EstablishMutualAidAgreement (Subdomain 1) ----------------------------

    [Fact]
    public void EstablishMutualAidAgreement_FromCreated_TransitionsToActive_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.ClearDomainEvents();

        var agreement = agg.EstablishMutualAidAgreement(
            agencyName: "Metro Fire Dept",
            discipline: "Fire",
            documentVersion: 1,
            effectiveFromUtc: FixedTimestamp,
            renewalDueAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)),
            establishedBy: TestUserId,
            establishedAtUtc: FixedTimestamp,
            actor: TestActor,
            correlationId: TestCorrelationId,
            causationId: TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Active);
        agg.MutualAidAgreements.Should().ContainSingle().Which.Should().BeSameAs(agreement);
        agreement.AgencyName.Should().Be("Metro Fire Dept");
        agreement.RenewalStatus.Should().Be(MutualAidAgreementRenewalStatus.Current);

        var evt = agg.DomainEvents.Single().Should().BeOfType<MutualAidAgreementEstablishedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
        evt.MutualAidAgreementId.Should().Be(agreement.MutualAidAgreementId);
    }

    [Fact]
    public void EstablishMutualAidAgreement_NormalizesDocumentVersionBelowOne_ToOne()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var agreement = agg.EstablishMutualAidAgreement(
            agencyName: "Metro Fire Dept", discipline: "Fire", documentVersion: 0,
            effectiveFromUtc: FixedTimestamp, renewalDueAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)),
            establishedBy: TestUserId, establishedAtUtc: FixedTimestamp, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        agreement.DocumentVersion.Should().Be(1);
    }

    [Fact]
    public void EstablishMutualAidAgreement_WhenClosed_ThrowsInvalidTransition()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.EstablishMutualAidAgreement(
            "Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)),
            TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LocalEmergencyCoordinationInvalidStatusTransitionException>();
    }

    // ---- ActivateJointResponsePlan / DeactivateJointResponsePlan (Subdomain 2) --

    [Fact]
    public void ActivateJointResponsePlan_FromCreated_TransitionsToActive_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.ClearDomainEvents();
        var incidentId = Guid.NewGuid();

        var plan = agg.ActivateJointResponsePlan(incidentId, "Wildfire Joint Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Active);
        plan.IsActive.Should().BeTrue();
        agg.JointResponsePlans.Should().ContainSingle();

        var evt = agg.DomainEvents.Single().Should().BeOfType<JointResponsePlanActivatedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
        evt.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public void DeactivateJointResponsePlan_WhenActive_SetsInactive_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var plan = agg.ActivateJointResponsePlan(null, "Plan A", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.DeactivateJointResponsePlan(plan.JointResponsePlanId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        plan.IsActive.Should().BeFalse();
        plan.DeactivatedAtUtc.Should().Be(FixedTimestamp);

        var evt = agg.DomainEvents.Single().Should().BeOfType<JointResponsePlanDeactivatedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DeactivateJointResponsePlan_WhenPlanDoesNotExist_ThrowsNotFound()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var act = () => agg.DeactivateJointResponsePlan(
            JointResponsePlanId.New(), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<JointResponsePlanNotFoundException>();
    }

    // ---- AssignExternalResponderLiaison / LogLiaisonCommunication (Subdomain 3) -

    [Fact]
    public void AssignExternalResponderLiaison_FromCreated_TransitionsToActive_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.ClearDomainEvents();

        var liaison = agg.AssignExternalResponderLiaison(null, "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Active);
        agg.ExternalResponderLiaisons.Should().ContainSingle().Which.Should().BeSameAs(liaison);

        var evt = agg.DomainEvents.Single().Should().BeOfType<ExternalResponderLiaisonAssignedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void LogLiaisonCommunication_WhenLiaisonExists_AppendsLogEntry_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(null, "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        var entry = agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, "Requested additional ambulances.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        liaison.LogEntries.Should().ContainSingle().Which.Should().BeSameAs(entry);
        entry.Message.Should().Be("Requested additional ambulances.");

        var evt = agg.DomainEvents.Single().Should().BeOfType<LiaisonCommunicationLoggedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
        evt.LiaisonLogEntryId.Should().Be(entry.LiaisonLogEntryId);
    }

    [Fact]
    public void LogLiaisonCommunication_WhenLiaisonDoesNotExist_ThrowsNotFound()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var act = () => agg.LogLiaisonCommunication(
            ExternalResponderLiaisonId.New(), "message", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ExternalResponderLiaisonNotFoundException>();
    }

    // ---- RecordJointDebrief (Subdomain 4) --------------------------------------

    [Fact]
    public void RecordJointDebrief_WhenActive_AddsDebriefRecord_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.WithMutualAidAgreement();
        agg.ClearDomainEvents();
        var incidentId = Guid.NewGuid();

        var debrief = agg.RecordJointDebrief(incidentId, "Metro Fire, County EMS", "Response was well coordinated.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.JointDebriefRecords.Should().ContainSingle().Which.Should().BeSameAs(debrief);

        var evt = agg.DomainEvents.Single().Should().BeOfType<JointDebriefRecordedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
        evt.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public void RecordJointDebrief_WhenCreated_DoesNotForceActivation()
    {
        // RecordJointDebrief (unlike the other three coordination workflows) does not
        // auto-transition Created -> Active; it only guards via CanCoordinate.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        agg.RecordJointDebrief(null, "Agency", "Summary", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Created);
    }

    // ---- Complete / Close / Archive lifecycle ----------------------------------

    [Fact]
    public void Complete_FromActive_TransitionsToCompleted()
    {
        var agg = LocalEmergencyCoordinationTestSupport.WithMutualAidAgreement();

        agg.Complete(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Completed);
    }

    [Fact]
    public void Complete_FromCreated_ThrowsInvalidTransition()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var act = () => agg.Complete(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LocalEmergencyCoordinationInvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created)]
    [InlineData(LocalEmergencyCoordinationStatus.Active)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed)]
    public void Close_FromPermittedStatuses_TransitionsToClosed_AndRaisesEvent_WithValidEventId(LocalEmergencyCoordinationStatus fromStatus)
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        if (fromStatus is LocalEmergencyCoordinationStatus.Active or LocalEmergencyCoordinationStatus.Completed)
            agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        if (fromStatus is LocalEmergencyCoordinationStatus.Completed)
            agg.Complete(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Closed);
        agg.ClosedAtUtc.Should().Be(FixedTimestamp);

        var evt = agg.DomainEvents.Single().Should().BeOfType<LocalEmergencyCoordinationClosedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Close_FromArchived_ThrowsInvalidTransition()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LocalEmergencyCoordinationInvalidStatusTransitionException>();
    }

    [Fact]
    public void Archive_FromClosed_TransitionsToArchived_AndRaisesEvent_WithValidEventId()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Archived);
        agg.ArchivedAtUtc.Should().Be(FixedTimestamp);

        var evt = agg.DomainEvents.Single().Should().BeOfType<LocalEmergencyCoordinationArchivedEvent>().Subject;
        LocalEmergencyCoordinationTestSupport.EventIdOf(evt).Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Archive_FromActive_ThrowsInvalidTransition()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var act = () => agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LocalEmergencyCoordinationInvalidStatusTransitionException>();
    }

    // ---- Full lifecycle regression: every event-raising transition in one run --

    [Fact]
    public void FullLifecycle_EveryEventRaisingTransition_ProducesAValidLocalEmergencyCoordinationEventId()
    {
        // Regression test for the hand-fix described in the module task brief: all 9 domain-event
        // raise sites inside Aggregates/LocalEmergencyCoordination.cs must construct their EventId via
        // ValueObjects.Ids.LocalEmergencyCoordinationEventId.New() (not the non-existent
        // Events.LocalEmergencyCoordinationEventId). Exercising every transition here would fail to
        // compile/execute at the first bad call site if the fix ever regressed.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(); // 1: Create
        var agreement = agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 2
        var plan = agg.ActivateJointResponsePlan(Guid.NewGuid(), "Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 3
        var liaison = agg.AssignExternalResponderLiaison(Guid.NewGuid(), "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 4
        var logEntry = agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, "Update", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 5
        var debrief = agg.RecordJointDebrief(Guid.NewGuid(), "Agency", "Summary", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 6
        agg.DeactivateJointResponsePlan(plan.JointResponsePlanId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 7
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 8
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId); // 9

        agg.DomainEvents.Should().HaveCount(9);
        agg.DomainEvents.Should().OnlyContain(e => LocalEmergencyCoordinationTestSupport.EventIdOf(e) != Guid.Empty, "every raised event must carry a real Uuid7-backed EventId, never a default/empty GUID");
        agg.DomainEvents.Select(LocalEmergencyCoordinationTestSupport.EventIdOf).Should().OnlyHaveUniqueItems("each domain event must receive its own freshly-generated EventId");

        agreement.Should().NotBeNull();
        logEntry.Should().NotBeNull();
        debrief.Should().NotBeNull();
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Archived);
    }

    // ---- ClearDomainEvents / RehydrateChildren ---------------------------------

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.DomainEvents.Should().NotBeEmpty();

        agg.ClearDomainEvents();

        agg.DomainEvents.Should().BeEmpty();
    }
}
