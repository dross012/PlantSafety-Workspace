// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalSecurityRulesTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the module's pure, stateless rules/policy classes:
//                  SabotageEscalationRules, SabotageRiskFeedPolicy, ControlChangeAuthorizationRules,
//                  and CyberPhysicalIncidentLifecycleRules. Grouped together (rather than one file
//                  per class) to keep test volume proportionate to this module's small policy
//                  surface -- each class is a handful of pure functions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Rules;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Rules;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Policies;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Rules;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class SabotageEscalationRulesTests
{
    [Fact]
    public void ShouldAutoEscalate_WithFewerThanThresholdDistinctSources_ReturnsFalse()
    {
        SabotageEscalationRules.ShouldAutoEscalate([SabotageIndicatorSource.ControlChange, SabotageIndicatorSource.ControlChange])
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldAutoEscalate_AtExactlyTheThreshold_ReturnsTrue()
    {
        SabotageEscalationRules.ShouldAutoEscalate([SabotageIndicatorSource.ControlChange, SabotageIndicatorSource.SensorIntegrity])
            .Should().BeTrue();
    }

    [Fact]
    public void ShouldAutoEscalate_WithEmptyCollection_ReturnsFalse()
    {
        SabotageEscalationRules.ShouldAutoEscalate([]).Should().BeFalse();
    }

    [Fact]
    public void ThresholdConstant_IsTwo()
    {
        SabotageEscalationRules.AutoEscalationDistinctSourceThreshold.Should().Be(2);
    }
}

public sealed class SabotageRiskFeedPolicyTests
{
    [Theory]
    [InlineData(SabotageIndicatorSeverity.Critical, "P1")]
    [InlineData(SabotageIndicatorSeverity.High, "P1")]
    [InlineData(SabotageIndicatorSeverity.Medium, "P2")]
    [InlineData(SabotageIndicatorSeverity.Low, "P3")]
    public void ResolveRiskManagementPriority_MapsSeverityToExpectedPriorityTier(SabotageIndicatorSeverity severity, string expectedPriority)
    {
        SabotageRiskFeedPolicy.ResolveRiskManagementPriority(severity).Should().Be(expectedPriority);
    }

    [Fact]
    public void ResolveRiskManagementPriority_CriticalAndHigh_BothMapToTheSameElevatedTier()
    {
        // "sabotage indicators feed RiskManagement at elevated priority" risk hook: Critical and
        // High both resolve to the platform's most urgent P1 tier.
        SabotageRiskFeedPolicy.ResolveRiskManagementPriority(SabotageIndicatorSeverity.Critical)
            .Should().Be(SabotageRiskFeedPolicy.ResolveRiskManagementPriority(SabotageIndicatorSeverity.High));
    }
}

public sealed class ControlChangeAuthorizationRulesTests
{
    [Fact]
    public void Classify_WithApprovedChangeRecord_ReturnsAuthorized()
    {
        ControlChangeAuthorizationRules.Classify(hasApprovedChangeRecord: true, hasKnownChangeSource: false)
            .Should().Be(ControlChangeAuthorizationStatus.Authorized);
    }

    [Fact]
    public void Classify_WithApprovedChangeRecordAndKnownSource_StillReturnsAuthorized()
    {
        ControlChangeAuthorizationRules.Classify(hasApprovedChangeRecord: true, hasKnownChangeSource: true)
            .Should().Be(ControlChangeAuthorizationStatus.Authorized);
    }

    [Fact]
    public void Classify_WithoutApprovalButKnownSource_ReturnsPendingReview()
    {
        ControlChangeAuthorizationRules.Classify(hasApprovedChangeRecord: false, hasKnownChangeSource: true)
            .Should().Be(ControlChangeAuthorizationStatus.PendingReview);
    }

    [Fact]
    public void Classify_WithoutApprovalAndUnknownSource_ReturnsUnauthorized()
    {
        ControlChangeAuthorizationRules.Classify(hasApprovedChangeRecord: false, hasKnownChangeSource: false)
            .Should().Be(ControlChangeAuthorizationStatus.Unauthorized);
    }
}

public sealed class CyberPhysicalIncidentLifecycleRulesTests
{
    [Theory]
    [InlineData(CyberPhysicalIncidentStatus.Open, CyberPhysicalIncidentStatus.Investigating, true)]
    [InlineData(CyberPhysicalIncidentStatus.Open, CyberPhysicalIncidentStatus.Closed, true)]
    [InlineData(CyberPhysicalIncidentStatus.Open, CyberPhysicalIncidentStatus.Contained, false)]
    [InlineData(CyberPhysicalIncidentStatus.Open, CyberPhysicalIncidentStatus.Resolved, false)]
    [InlineData(CyberPhysicalIncidentStatus.Investigating, CyberPhysicalIncidentStatus.Contained, true)]
    [InlineData(CyberPhysicalIncidentStatus.Investigating, CyberPhysicalIncidentStatus.Closed, true)]
    [InlineData(CyberPhysicalIncidentStatus.Investigating, CyberPhysicalIncidentStatus.Open, false)]
    [InlineData(CyberPhysicalIncidentStatus.Contained, CyberPhysicalIncidentStatus.Resolved, true)]
    [InlineData(CyberPhysicalIncidentStatus.Contained, CyberPhysicalIncidentStatus.Closed, true)]
    [InlineData(CyberPhysicalIncidentStatus.Resolved, CyberPhysicalIncidentStatus.Closed, true)]
    [InlineData(CyberPhysicalIncidentStatus.Resolved, CyberPhysicalIncidentStatus.Open, false)]
    [InlineData(CyberPhysicalIncidentStatus.Closed, CyberPhysicalIncidentStatus.Open, false)]
    [InlineData(CyberPhysicalIncidentStatus.Closed, CyberPhysicalIncidentStatus.Investigating, false)]
    public void IsValidTransition_ReturnsExpectedResult(CyberPhysicalIncidentStatus from, CyberPhysicalIncidentStatus to, bool expected)
    {
        CyberPhysicalIncidentLifecycleRules.IsValidTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void AllowedTransitions_FromClosed_IsTerminalWithNoOutboundTransitions()
    {
        CyberPhysicalIncidentLifecycleRules.AllowedTransitions[CyberPhysicalIncidentStatus.Closed].Should().BeEmpty();
    }
}
