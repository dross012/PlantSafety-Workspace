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
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Industries;

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

    [Fact]
    public void ComplianceRiskEngine_ComputeSeverityAwareComplianceScore_BuildsBaseScoreThenAdjusts()
    {
        // A freshly built facility (nothing completed) is NonCompliant, which the Catastrophic cap
        // (which only ever caps -down- to PartiallyCompliant) leaves unaffected -- confirms this
        // wiring calls through to the real base-score builder rather than hand-waving a fixed tier.
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "Plant 1", "Chemical Manufacturing", "Houston, TX");
        ComplianceScore baseScore = ComplianceRiskEngine.ComputeComplianceScore(facility);
        Assert.Equal(ComplianceTier.NonCompliant, baseScore.ComplianceTier);

        SeverityAwareComplianceScore result = ComplianceRiskEngine.ComputeSeverityAwareComplianceScore(facility, SeverityLevel.Catastrophic);

        Assert.Equal(facility.FacilityId, result.FacilityId);
        Assert.Equal(baseScore, result.BaseComplianceScore);
        Assert.Equal(ComplianceTier.NonCompliant, result.AdjustedComplianceTier);
    }

    [Fact]
    public void ComplianceRiskEngine_ComputeSeverityAwareComplianceScore_FullyCompliantFacility_CatastrophicCapsToPartiallyCompliant()
    {
        FacilityProfile draft = FacilityProfileBuilder.Build(Guid.NewGuid(), "Plant 1", "Chemical Manufacturing", "Houston, TX");
        var allCodes = draft.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet(StringComparer.Ordinal);
        FacilityProfile fullyCompliant = FacilityProfileBuilder.Build(draft.FacilityId, "Plant 1", "Chemical Manufacturing", "Houston, TX", allCodes);
        Assert.Equal(ComplianceTier.FullyCompliant, ComplianceRiskEngine.ComputeComplianceScore(fullyCompliant).ComplianceTier);

        SeverityAwareComplianceScore result = ComplianceRiskEngine.ComputeSeverityAwareComplianceScore(fullyCompliant, SeverityLevel.Catastrophic);

        Assert.Equal(ComplianceTier.PartiallyCompliant, result.AdjustedComplianceTier);
    }

    [Fact]
    public void ComplianceRiskEngine_ComputeSeverityAwareRiskScore_BuildsBaseScoreThenAdjusts()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "Plant 1", "Chemical Manufacturing", "Houston, TX");
        RiskScore baseScore = ComplianceRiskEngine.ComputeRiskScore(facility);

        SeverityAwareRiskScore result = ComplianceRiskEngine.ComputeSeverityAwareRiskScore(facility, SeverityLevel.Severe, employeeInjuryCount: 0, machineFailureCount: 0, eventChainLength: 1);

        Assert.Equal(facility.FacilityId, result.FacilityId);
        Assert.Equal(baseScore, result.BaseRiskScore);
        bool expectHigh = baseScore.OverallRiskLevel < RiskLevel.High;
        Assert.Equal(expectHigh ? RiskLevel.High : baseScore.OverallRiskLevel, result.AdjustedOverallRiskLevel);
    }

    [Fact]
    public void IComplianceRiskService_ComputeSeverityAwareScores_DelegatesToEngine()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "Plant 1", "Chemical Manufacturing", "Houston, TX");
        IComplianceRiskService service = new ComplianceRiskService();

        SeverityAwareComplianceScore complianceResult = service.ComputeSeverityAwareComplianceScore(facility, SeverityLevel.Moderate);
        SeverityAwareRiskScore riskResult = service.ComputeSeverityAwareRiskScore(facility, SeverityLevel.Moderate, 0, 0, 1);

        Assert.Equal(ComplianceRiskEngine.ComputeSeverityAwareComplianceScore(facility, SeverityLevel.Moderate), complianceResult);
        Assert.Equal(ComplianceRiskEngine.ComputeSeverityAwareRiskScore(facility, SeverityLevel.Moderate, 0, 0, 1), riskResult);
    }
}
