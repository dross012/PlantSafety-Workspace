// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           SeverityAwareScoringTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.10 (Path C: Operational Modernization) unit
//                  tests for SeverityImpactRules, SeverityAwareRiskScoreBuilder,
//                  and SeverityAwareComplianceScoreBuilder.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class SeverityAwareScoringTests
{
    [Theory]
    [InlineData(SeverityLevel.Minor, RiskLevel.Low)]
    [InlineData(SeverityLevel.Moderate, RiskLevel.Medium)]
    [InlineData(SeverityLevel.Severe, RiskLevel.High)]
    [InlineData(SeverityLevel.Catastrophic, RiskLevel.Critical)]
    public void SeverityImpactRules_SeverityFloor_ReturnsExpectedFloor(SeverityLevel severity, RiskLevel expectedFloor)
    {
        Assert.Equal(expectedFloor, SeverityImpactRules.SeverityFloor(severity));
    }

    [Fact]
    public void SeverityImpactRules_AdjustRiskLevel_NeverLowersBaseline()
    {
        RiskLevel adjusted = SeverityImpactRules.AdjustRiskLevel(RiskLevel.High, SeverityLevel.Minor, employeeInjuryCount: 0, machineFailureCount: 0, eventChainLength: 1);

        Assert.Equal(RiskLevel.High, adjusted);
    }

    [Fact]
    public void SeverityImpactRules_AdjustRiskLevel_RaisesToSeverityFloor()
    {
        RiskLevel adjusted = SeverityImpactRules.AdjustRiskLevel(RiskLevel.Low, SeverityLevel.Catastrophic, employeeInjuryCount: 0, machineFailureCount: 0, eventChainLength: 1);

        Assert.Equal(RiskLevel.Critical, adjusted);
    }

    [Fact]
    public void SeverityImpactRules_AdjustRiskLevel_EscalatesOneTierOnInjuryThreshold()
    {
        RiskLevel adjusted = SeverityImpactRules.AdjustRiskLevel(RiskLevel.Low, SeverityLevel.Minor, employeeInjuryCount: 2, machineFailureCount: 0, eventChainLength: 1);

        Assert.Equal(RiskLevel.Medium, adjusted);
    }

    [Fact]
    public void SeverityImpactRules_AdjustRiskLevel_EscalationCapsAtCritical()
    {
        RiskLevel adjusted = SeverityImpactRules.AdjustRiskLevel(RiskLevel.Critical, SeverityLevel.Catastrophic, employeeInjuryCount: 5, machineFailureCount: 5, eventChainLength: 5);

        Assert.Equal(RiskLevel.Critical, adjusted);
    }

    [Fact]
    public void SeverityImpactRules_AdjustComplianceTier_CatastrophicCapsAtPartiallyCompliant()
    {
        ComplianceTier adjusted = SeverityImpactRules.AdjustComplianceTier(ComplianceTier.FullyCompliant, SeverityLevel.Catastrophic);

        Assert.Equal(ComplianceTier.PartiallyCompliant, adjusted);
    }

    [Fact]
    public void SeverityImpactRules_AdjustComplianceTier_NonCatastrophicLeavesTierUnchanged()
    {
        ComplianceTier adjusted = SeverityImpactRules.AdjustComplianceTier(ComplianceTier.FullyCompliant, SeverityLevel.Severe);

        Assert.Equal(ComplianceTier.FullyCompliant, adjusted);
    }

    [Fact]
    public void SeverityAwareRiskScoreBuilder_Build_AdjustsOverallRiskLevel()
    {
        var facilityId = Guid.NewGuid();
        var baseScore = new RiskScore(facilityId, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low);

        SeverityAwareRiskScore result = SeverityAwareRiskScoreBuilder.Build(baseScore, SeverityLevel.Severe, employeeInjuryCount: 0, machineFailureCount: 0, eventChainLength: 1);

        Assert.Equal(facilityId, result.FacilityId);
        Assert.Equal(RiskLevel.High, result.AdjustedOverallRiskLevel);
        Assert.Same(baseScore, result.BaseRiskScore);
    }

    [Fact]
    public void SeverityAwareComplianceScoreBuilder_Build_AdjustsComplianceTier()
    {
        var facilityId = Guid.NewGuid();
        var baseScore = new ComplianceScore(facilityId, 10, 10, 0, 100d, ComplianceTier.FullyCompliant, "All complete.");

        SeverityAwareComplianceScore result = SeverityAwareComplianceScoreBuilder.Build(baseScore, SeverityLevel.Catastrophic);

        Assert.Equal(facilityId, result.FacilityId);
        Assert.Equal(ComplianceTier.PartiallyCompliant, result.AdjustedComplianceTier);
        Assert.Same(baseScore, result.BaseComplianceScore);
    }
}
