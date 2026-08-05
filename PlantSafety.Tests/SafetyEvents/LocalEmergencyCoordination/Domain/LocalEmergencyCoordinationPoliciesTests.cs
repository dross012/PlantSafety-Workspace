// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Domain
//  Layer:          Tests (Unit)
//  File:           LocalEmergencyCoordinationPoliciesTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the module's pure domain policies (archival eligibility, mutual
//                  aid renewal/currency, liaison gap escalation) and lifecycle-transition rules.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Rules;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Domain;

public sealed class LocalEmergencyCoordinationPoliciesTests : UnitTestBase
{
    // ---- LocalEmergencyCoordinationArchivalPolicy ------------------------------

    [Fact]
    public void ArchivalPolicy_WhenClosed_IsEligible()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        LocalEmergencyCoordinationArchivalPolicy.IsEligibleForArchival(agg).Should().BeTrue();
    }

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created)]
    [InlineData(LocalEmergencyCoordinationStatus.Active)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived)]
    public void ArchivalPolicy_WhenNotClosed_IsNotEligible(LocalEmergencyCoordinationStatus status)
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        if (status == LocalEmergencyCoordinationStatus.Active)
            agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        if (status == LocalEmergencyCoordinationStatus.Archived)
        {
            agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
            agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        }

        LocalEmergencyCoordinationArchivalPolicy.IsEligibleForArchival(agg).Should().BeFalse();
    }

    // ---- MutualAidAgreementRenewalPolicy ----------------------------------------

    [Fact]
    public void RenewalPolicy_WhenFarFromDue_IsCurrent()
    {
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(200));

        MutualAidAgreementRenewalPolicy.Evaluate(due, FixedTimestamp).Should().Be(MutualAidAgreementRenewalStatus.Current);
    }

    [Fact]
    public void RenewalPolicy_WithinReviewWindow_IsPendingRenewal()
    {
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(MutualAidAgreementRenewalPolicy.ReviewWindowDays - 1));

        MutualAidAgreementRenewalPolicy.Evaluate(due, FixedTimestamp).Should().Be(MutualAidAgreementRenewalStatus.PendingRenewal);
    }

    [Fact]
    public void RenewalPolicy_AtOrPastDueDate_IsExpired()
    {
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(-1));

        MutualAidAgreementRenewalPolicy.Evaluate(due, FixedTimestamp).Should().Be(MutualAidAgreementRenewalStatus.Expired);
    }

    [Fact]
    public void RenewalPolicy_ExactlyAtReviewWindowBoundary_IsPendingRenewal()
    {
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(MutualAidAgreementRenewalPolicy.ReviewWindowDays));

        MutualAidAgreementRenewalPolicy.Evaluate(due, FixedTimestamp).Should().Be(MutualAidAgreementRenewalStatus.PendingRenewal);
    }

    // ---- LiaisonGapEscalationPolicy ---------------------------------------------

    [Fact]
    public void GapEscalationPolicy_WhenNoPriorCommunication_RequiresEscalation()
    {
        LiaisonGapEscalationPolicy.RequiresEscalation(null, FixedTimestamp).Should().BeTrue();
    }

    [Fact]
    public void GapEscalationPolicy_WithinSilenceWindow_DoesNotRequireEscalation()
    {
        var last = UtcTimestamp.From(FixedUtcNow.Subtract(LiaisonGapEscalationPolicy.MaxSilenceWindow).AddMinutes(1));

        LiaisonGapEscalationPolicy.RequiresEscalation(last, FixedTimestamp).Should().BeFalse();
    }

    [Fact]
    public void GapEscalationPolicy_BeyondSilenceWindow_RequiresEscalation()
    {
        var last = UtcTimestamp.From(FixedUtcNow.Subtract(LiaisonGapEscalationPolicy.MaxSilenceWindow).AddMinutes(-1));

        LiaisonGapEscalationPolicy.RequiresEscalation(last, FixedTimestamp).Should().BeTrue();
    }

    // ---- LocalEmergencyCoordinationLifecycleRules -------------------------------

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Closed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived, false)]
    public void CanActivate_OnlyFromCreated(LocalEmergencyCoordinationStatus status, bool expected)
        => LocalEmergencyCoordinationLifecycleRules.CanActivate(status).Should().Be(expected);

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Closed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived, false)]
    public void CanCoordinate_OnlyFromCreatedOrActive(LocalEmergencyCoordinationStatus status, bool expected)
        => LocalEmergencyCoordinationLifecycleRules.CanCoordinate(status).Should().Be(expected);

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, false)]
    public void CanComplete_OnlyFromActive(LocalEmergencyCoordinationStatus status, bool expected)
        => LocalEmergencyCoordinationLifecycleRules.CanComplete(status).Should().Be(expected);

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Closed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived, false)]
    public void CanClose_FromCreatedActiveOrCompleted(LocalEmergencyCoordinationStatus status, bool expected)
        => LocalEmergencyCoordinationLifecycleRules.CanClose(status).Should().Be(expected);

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Closed, true)]
    [InlineData(LocalEmergencyCoordinationStatus.Created, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, false)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived, false)]
    public void CanArchive_OnlyFromClosed(LocalEmergencyCoordinationStatus status, bool expected)
        => LocalEmergencyCoordinationLifecycleRules.CanArchive(status).Should().Be(expected);
}
