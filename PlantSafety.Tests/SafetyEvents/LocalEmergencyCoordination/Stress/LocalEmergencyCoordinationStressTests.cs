// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Stress
//  Layer:          Tests (Stress)
//  File:           LocalEmergencyCoordinationStressTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Stress tests exercising large in-memory volumes against a single aggregate
//                  instance (a burst of liaison communications during a prolonged live incident,
//                  and concurrent-style rapid-fire coordination workflow calls), verifying
//                  collection integrity, distinct identity generation, and graceful behavior
//                  under sustained load without corruption -- no I/O, no network.
// ============================================================================
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Stress;

public sealed class LocalEmergencyCoordinationStressTests : UnitTestBase
{
    [Fact]
    public void LiaisonCommunicationBurst_OneThousandEntries_MaintainsCollectionIntegrityAndUniqueIdentities()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(Guid.NewGuid(), "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        for (var i = 0; i < 1000; i++)
        {
            agg.LogLiaisonCommunication(liaison.ExternalResponderLiaisonId, $"Update #{i}", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }

        liaison.LogEntries.Should().HaveCount(1000);
        liaison.LogEntries.Select(e => e.LiaisonLogEntryId.Value).Should().OnlyHaveUniqueItems();
        liaison.LogEntries.Select(e => e.Message).Distinct().Should().HaveCount(1000);
    }

    [Fact]
    public void SustainedMultiSubdomainBurst_FiveHundredOperationsAcrossAllFourSubdomains_ProducesConsistentAggregateState()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var due = UtcTimestamp.From(FixedUtcNow.AddYears(1));

        for (var i = 0; i < 125; i++)
        {
            agg.EstablishMutualAidAgreement($"Agency {i}", "Fire", 1, FixedTimestamp, due, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
            agg.ActivateJointResponsePlan(Guid.NewGuid(), $"Plan {i}", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
            agg.AssignExternalResponderLiaison(Guid.NewGuid(), $"Agency {i}", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
            agg.RecordJointDebrief(Guid.NewGuid(), $"Agency {i}", $"Summary {i}", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }

        agg.MutualAidAgreements.Should().HaveCount(125);
        agg.JointResponsePlans.Should().HaveCount(125);
        agg.ExternalResponderLiaisons.Should().HaveCount(125);
        agg.JointDebriefRecords.Should().HaveCount(125);

        // 500 operations => 500 domain events, every one with a distinct Uuid7 EventId -- a
        // regression here would indicate identity collisions or lost writes under sustained load.
        agg.DomainEvents.Should().HaveCount(501); // +1 for the initial Create event
        agg.DomainEvents.Select(LocalEmergencyCoordinationTestSupport.EventIdOf).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ManySmallCases_EachIndependentlyStressed_DoNotShareState()
    {
        var cases = Enumerable.Range(0, 100).Select(_ => LocalEmergencyCoordinationTestSupport.NewCase()).ToList();

        foreach (var c in cases)
        {
            c.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }

        cases.Select(c => c.LocalEmergencyCoordinationId.Value).Should().OnlyHaveUniqueItems();
        cases.Should().OnlyContain(c => c.MutualAidAgreements.Count == 1);
    }
}
