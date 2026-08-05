// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Acceptance
//  Layer:          Tests (Acceptance / UAT)
//  File:           LocalEmergencyCoordinationAcceptanceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    User-acceptance-style tests phrased as Given/When/Then coordinator stories,
//                  validating the module delivers the business capabilities described in its
//                  module brief: mutual aid agreement tracking, joint response planning, live
//                  incident liaison logging, and post-incident joint debriefs. Isolated,
//                  deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Acceptance;

public sealed class LocalEmergencyCoordinationAcceptanceTests : UnitTestBase
{
    [Fact]
    public void GivenACoordinator_WhenTheyEstablishMutualAid_ThenTheAgreementCurrencyIsTracked()
    {
        // Given a coordinator opens a case
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        // When they establish a mutual aid agreement due for renewal next year
        var agreement = agg.EstablishMutualAidAgreement(
            "Metro Fire Dept", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)),
            TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // Then the agreement currency (renewal status) KPI reflects "Current"
        agreement.RenewalStatus.Should().Be(MutualAidAgreementRenewalStatus.Current);
    }

    [Fact]
    public void GivenAJointResponsePlan_WhenTheIncidentConcludes_ThenTheCoordinatorCanRecordADebrief()
    {
        // Given an active joint response plan for an incident
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var incidentId = Guid.NewGuid();
        var plan = agg.ActivateJointResponsePlan(incidentId, "Joint Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // When the incident concludes and the plan is deactivated
        agg.DeactivateJointResponsePlan(plan.JointResponsePlanId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // Then the coordinator can record a joint debrief referencing the same incident
        var debrief = agg.RecordJointDebrief(incidentId, "Metro Fire Dept", "Debrief summary.", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // And the joint debrief completion rate KPI can see the completed plan/debrief pairing
        var rate = new PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Services.LocalEmergencyCoordinationService()
            .ComputeJointDebriefCompletionRate(agg.JointResponsePlans, agg.JointDebriefRecords);

        rate.Should().Be(1.0);
        debrief.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public void GivenALiveIncidentLiaison_WhenNoCommunicationIsLoggedForTooLong_ThenEscalationIsRequired()
    {
        // Given a liaison assigned to a live incident with no communications yet logged
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(Guid.NewGuid(), "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // When the risk hook evaluates the silence window
        var requiresEscalation = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.LiaisonGapEscalationPolicy
            .RequiresEscalation(null, FixedTimestamp);

        // Then escalation through RiskManagement is required (module risk hook)
        requiresEscalation.Should().BeTrue();
        liaison.LogEntries.Should().BeEmpty();
    }

    [Fact]
    public void GivenAClosedCase_WhenComplianceRetentionApplies_ThenTheCoordinatorCanArchiveIt()
    {
        // Given a closed coordination case
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // When EPCRA/LEPC compliance retention applies
        var eligible = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.LocalEmergencyCoordinationArchivalPolicy.IsEligibleForArchival(agg);
        eligible.Should().BeTrue();

        // Then the coordinator can archive the case
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Archived);
    }
}
