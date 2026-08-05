// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Performance
//  Layer:          Tests (Performance)
//  File:           LocalEmergencyCoordinationPerformanceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Performance sanity checks for pure in-memory domain operations, using the
//                  Command/Query Handler latency envelope from the Performance & Stress Rules and
//                  Design Manual (P95 <= 200ms) as a generous CI-safe ceiling for a batch of 500
//                  in-process aggregate mutations -- no I/O, no network, so actual latency is
//                  expected to be several orders of magnitude under the ceiling; the wide margin
//                  keeps this deterministic under CI load rather than flaking on shared runners.
// ============================================================================
using System.Diagnostics;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Performance;

public sealed class LocalEmergencyCoordinationPerformanceTests : UnitTestBase
{
    [Fact]
    public void EstablishingFiveHundredMutualAidAgreements_CompletesWellWithinHandlerLatencyCeiling()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var due = UtcTimestamp.From(FixedUtcNow.AddYears(1));

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 500; i++)
        {
            agg.EstablishMutualAidAgreement($"Agency {i}", "Fire", 1, FixedTimestamp, due, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }
        stopwatch.Stop();

        agg.MutualAidAgreements.Should().HaveCount(500);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "500 pure in-memory aggregate mutations must comfortably clear the 200ms Command/Query Handler P95 ceiling");
    }

    [Fact]
    public void ActivatingAndDeactivatingTwoHundredJointResponsePlans_CompletesWellWithinHandlerLatencyCeiling()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 200; i++)
        {
            var plan = agg.ActivateJointResponsePlan(Guid.NewGuid(), $"Plan {i}", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
            agg.DeactivateJointResponsePlan(plan.JointResponsePlanId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }
        stopwatch.Stop();

        agg.JointResponsePlans.Should().HaveCount(200);
        agg.JointResponsePlans.Should().OnlyContain(p => !p.IsActive);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public void RenewalPolicyEvaluation_AcrossOneThousandAgreements_CompletesWellWithinHandlerLatencyCeiling()
    {
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(30));

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.MutualAidAgreementRenewalPolicy.Evaluate(due, FixedTimestamp);
        }
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "a pure predicate policy evaluated 1000 times must be effectively instantaneous");
    }
}
