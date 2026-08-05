// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Scenario
//  Layer:          Tests (Scenario)
//  File:           LocalEmergencyCoordinationScenarioTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    End-to-end realistic workflow scenario for LocalEmergencyCoordination:
//                  a coordinator opens a case for a wildfire incident, establishes a mutual aid
//                  agreement, activates a joint response plan, assigns a liaison, logs
//                  communications during the live incident, deactivates the plan once the incident
//                  concludes, records the joint debrief, then closes and archives the case.
//                  Exercises every one of the module's four subdomains in a single narrative.
//                  Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Scenario;

public sealed class LocalEmergencyCoordinationScenarioTests : UnitTestBase
{
    [Fact]
    public void FullWildfireCoordinationScenario_ProgressesThroughEverySubdomainAndLifecycleStage()
    {
        var incidentId = Guid.NewGuid();

        // 1. Coordinator opens the case for the incident.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(incidentId: incidentId, caseName: "2026 Ridge Wildfire Mutual Aid");
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Created);

        // 2. Mutual aid is established with the regional fire authority.
        var agreement = agg.EstablishMutualAidAgreement(
            "Ridge County Fire Authority", "Fire", 3, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)),
            TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Active);

        // 3. A joint response plan is activated for the incident.
        var plan = agg.ActivateJointResponsePlan(incidentId, "Ridge Wildfire Joint Suppression Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        plan.IsActive.Should().BeTrue();

        // 4. An internal liaison is assigned to the fire authority for the live incident.
        var liaison = agg.AssignExternalResponderLiaison(incidentId, "Ridge County Fire Authority", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // 5. Several communications are logged during the live incident.
        agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, "Requested two additional strike teams.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, "Strike teams en route, ETA 20 minutes.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        liaison.LogEntries.Should().HaveCount(2);

        // 6. The joint response plan is deactivated once the fire is contained.
        agg.DeactivateJointResponsePlan(plan.JointResponsePlanId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        plan.IsActive.Should().BeFalse();

        // 7. A joint post-incident debrief is recorded with the responding agencies.
        var debrief = agg.RecordJointDebrief(incidentId, "Ridge County Fire Authority", "Mutual aid response effective; comms cadence held throughout.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // 8. The coordination case is completed, closed, and archived for EPCRA/LEPC retention.
        agg.Complete(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Completed);

        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Closed);

        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Archived);

        // Full narrative assertions across all four child entity collections.
        agg.MutualAidAgreements.Should().ContainSingle().Which.Should().BeSameAs(agreement);
        agg.JointResponsePlans.Should().ContainSingle().Which.Should().BeSameAs(plan);
        agg.ExternalResponderLiaisons.Should().ContainSingle().Which.Should().BeSameAs(liaison);
        agg.JointDebriefRecords.Should().ContainSingle().Which.Should().BeSameAs(debrief);
    }

    [Fact]
    public void MultipleAgenciesScenario_TracksIndependentAgreementsUnderTheSameCase()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(caseName: "Regional Multi-Agency Coordination");

        agg.EstablishMutualAidAgreement("Fire Authority", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.EstablishMutualAidAgreement("Police Department", "Police", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.EstablishMutualAidAgreement("County EMS", "EMS", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.MutualAidAgreements.Should().HaveCount(3);
        agg.MutualAidAgreements.Select(a => a.Discipline).Should().BeEquivalentTo(new[] { "Fire", "Police", "EMS" });
    }
}
