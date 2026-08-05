// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Recovery
//  Layer:          Tests (Recovery)
//  File:           LocalEmergencyCoordinationRecoveryTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Recovery tests: aggregate rehydration of child entity collections
//                  (RehydrateChildren, ExternalResponderLiaison.RehydrateLogEntries) restores full
//                  read state after a simulated process restart / cache eviction, ClearDomainEvents
//                  leaves persisted state untouched so a crash-after-dispatch-before-clear recovers
//                  cleanly on next load, and a case can resume mid-workflow after "recovery" without
//                  losing prior coordination history. Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Recovery;

public sealed class LocalEmergencyCoordinationRecoveryTests : UnitTestBase
{
    [Fact]
    public void RehydrateChildren_RestoresAllFourChildCollections_AfterSimulatedProcessRestart()
    {
        // Build up an aggregate as if it had been "in flight" before a process restart.
        var original = LocalEmergencyCoordinationTestSupport.NewCase();
        var agreement = original.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var plan = original.ActivateJointResponsePlan(Guid.NewGuid(), "Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var liaison = original.AssignExternalResponderLiaison(Guid.NewGuid(), "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var debrief = original.RecordJointDebrief(Guid.NewGuid(), "Agency", "Summary", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // Simulate recovery: a freshly-constructed aggregate (as the Infrastructure repository would
        // produce from a DB round-trip after a restart) rehydrated with the same child rows.
        var recovered = LocalEmergencyCoordinationTestSupport.NewCase();
        typeof(PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination)
            .GetMethod("RehydrateChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(recovered, [new[] { agreement }, new[] { plan }, new[] { liaison }, new[] { debrief }]);

        recovered.MutualAidAgreements.Should().ContainSingle().Which.Should().BeSameAs(agreement);
        recovered.JointResponsePlans.Should().ContainSingle().Which.Should().BeSameAs(plan);
        recovered.ExternalResponderLiaisons.Should().ContainSingle().Which.Should().BeSameAs(liaison);
        recovered.JointDebriefRecords.Should().ContainSingle().Which.Should().BeSameAs(debrief);
    }

    [Fact]
    public void ExternalResponderLiaison_RehydrateLogEntries_RestoresCommunicationHistory()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(null, "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var entry = agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, "Requested resources.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // Simulate a fresh read-side projection: the Infrastructure repository rehydrates the
        // liaison's log entries from storage via the internal ORM-rehydration hook.
        typeof(PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities.ExternalResponderLiaison)
            .GetMethod("RehydrateLogEntries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(liaison, [new[] { entry }]);

        liaison.LogEntries.Should().ContainSingle().Which.Message.Should().Be("Requested resources.");
    }

    [Fact]
    public void ClearDomainEvents_AfterOutboxDispatch_LeavesPersistedAggregateStateUnaffected()
    {
        // ClearDomainEvents only clears the in-memory pending-event buffer -- if a crash occurs
        // after the outbox dispatch but before ClearDomainEvents(), the next load must still reflect
        // the correct persisted state (recovery does not depend on domain-event buffer contents).
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.ClearDomainEvents();

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Active);
        agg.MutualAidAgreements.Should().ContainSingle();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ResumedCase_AfterRecovery_CanContinueMidWorkflowWithoutLosingPriorHistory()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents(); // simulate: events already dispatched before the "crash"

        // Recovery resumes the case: a further coordination workflow is added post-recovery.
        var plan = agg.ActivateJointResponsePlan(null, "Recovery-Resumed Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.MutualAidAgreements.Should().ContainSingle("prior history must survive recovery");
        agg.JointResponsePlans.Should().ContainSingle().Which.Should().BeSameAs(plan);
    }
}
