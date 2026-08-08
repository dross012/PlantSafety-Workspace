// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           ComplianceRiskTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Compliance & Risk subsystem:
//                  ComplianceScoreBuilder, RiskScoreBuilder,
//                  ComplianceTrendBuilder, RiskTrendBuilder,
//                  ComplianceDashboardMetadataBuilder,
//                  RiskDashboardMetadataBuilder, ComplianceReadinessBuilder,
//                  RiskReadinessBuilder, and ComplianceRiskEngine. Verifies
//                  tier/level thresholds, trend direction classification,
//                  readiness at 0%/100% completion, and totality across all
//                  40 industries.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class ComplianceRiskTests
{
    [Theory]
    [InlineData(0d, ComplianceTier.NonCompliant)]
    [InlineData(1d, ComplianceTier.PartiallyCompliant)]
    [InlineData(74.9d, ComplianceTier.PartiallyCompliant)]
    [InlineData(75d, ComplianceTier.SubstantiallyCompliant)]
    [InlineData(99.9d, ComplianceTier.SubstantiallyCompliant)]
    [InlineData(100d, ComplianceTier.FullyCompliant)]
    public void ComplianceScoreBuilder_TierFor_UsesDocumentedThresholds(double percentComplete, ComplianceTier expectedTier)
    {
        Assert.Equal(expectedTier, ComplianceScoreBuilder.TierFor(percentComplete));
    }

    [Fact]
    public void ComplianceScoreBuilder_Build_MatchesFacilityComplianceProfile()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        ComplianceScore score = ComplianceScoreBuilder.Build(facility);

        Assert.Equal(facility.ComplianceProfile.RequiredElements.Count, score.RequiredElementsCount);
        Assert.Equal(facility.ComplianceProfile.CompletedElements.Count, score.CompletedElementsCount);
        Assert.Equal(facility.ComplianceProfile.MissingElements.Count, score.MissingElementsCount);
        Assert.Equal(facility.ComplianceProfile.ComplianceScorePercent, score.PercentComplete);
    }

    [Fact]
    public void RiskScoreBuilder_Build_OverallIsMaxOfFourDimensions()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), industry.Name, industry.Name, "Loc");
            RiskScore score = RiskScoreBuilder.Build(facility);

            RiskLevel expectedMax = new[] { score.HazardRiskLevel, score.ProgramRiskLevel, score.RegulatorRiskLevel, score.EventRiskLevel }.Max();

            Assert.Equal(expectedMax, score.OverallRiskLevel);
        }
    }

    [Fact]
    public void RiskScoreBuilder_NoCompletedElements_ProgramRiskIsHigh()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        RiskScore score = RiskScoreBuilder.Build(facility);

        Assert.Equal(RiskLevel.High, score.ProgramRiskLevel);
    }

    [Fact]
    public void RiskScoreBuilder_AllElementsCompleted_ProgramRiskIsLow()
    {
        FacilityProfile draft = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        var allCodes = draft.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet();
        FacilityProfile facility = FacilityProfileBuilder.Build(draft.FacilityId, "X", "Chemical Manufacturing", "Loc", allCodes);

        RiskScore score = RiskScoreBuilder.Build(facility);

        Assert.Equal(RiskLevel.Low, score.ProgramRiskLevel);
    }

    [Fact]
    public void RegulatorCategoryRiskCatalog_ForCategory_IsTotal()
    {
        foreach (RegulatorCategory category in Enum.GetValues<RegulatorCategory>())
        {
            RegulatorCategoryRiskCatalog.ForCategory(category);
        }
    }

    [Fact]
    public void ComplianceTrendBuilder_Build_DifferentFacilityIds_Throws()
    {
        var scoreA = new ComplianceScore(Guid.NewGuid(), 10, 0, 10, 0d, ComplianceTier.NonCompliant, "n/a");
        var scoreB = new ComplianceScore(Guid.NewGuid(), 10, 10, 0, 100d, ComplianceTier.FullyCompliant, "n/a");

        Assert.Throws<ArgumentException>(() => ComplianceTrendBuilder.Build(scoreA, scoreB));
    }

    [Theory]
    [InlineData(0d, 50d, ComplianceTrendDirection.Improving)]
    [InlineData(50d, 50d, ComplianceTrendDirection.Stable)]
    [InlineData(50d, 0d, ComplianceTrendDirection.Declining)]
    public void ComplianceTrendBuilder_Build_ClassifiesDirectionFromDelta(double lastPercent, double currentPercent, ComplianceTrendDirection expected)
    {
        Guid facilityId = Guid.NewGuid();
        var previous = new ComplianceScore(facilityId, 10, 0, 10, lastPercent, ComplianceScoreBuilder.TierFor(lastPercent), "n/a");
        var current = new ComplianceScore(facilityId, 10, 0, 10, currentPercent, ComplianceScoreBuilder.TierFor(currentPercent), "n/a");

        ComplianceTrend trend = ComplianceTrendBuilder.Build(previous, current);

        Assert.Equal(currentPercent - lastPercent, trend.Delta);
        Assert.Equal(expected, trend.Direction);
    }

    [Theory]
    [InlineData(RiskLevel.High, RiskLevel.Low, RiskTrendDirection.Improving)]
    [InlineData(RiskLevel.Low, RiskLevel.Low, RiskTrendDirection.Stable)]
    [InlineData(RiskLevel.Low, RiskLevel.Critical, RiskTrendDirection.Worsening)]
    public void RiskTrendBuilder_Build_ClassifiesDirectionFromOrdinalDelta(RiskLevel lastLevel, RiskLevel currentLevel, RiskTrendDirection expected)
    {
        Guid facilityId = Guid.NewGuid();
        var previous = new RiskScore(facilityId, lastLevel, lastLevel, lastLevel, lastLevel, lastLevel);
        var current = new RiskScore(facilityId, currentLevel, currentLevel, currentLevel, currentLevel, currentLevel);

        RiskTrend trend = RiskTrendBuilder.Build(previous, current);

        Assert.Equal(expected, trend.Direction);
    }

    [Fact]
    public void RiskTrendBuilder_Build_DifferentFacilityIds_Throws()
    {
        var scoreA = new RiskScore(Guid.NewGuid(), RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low);
        var scoreB = new RiskScore(Guid.NewGuid(), RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low, RiskLevel.Low);

        Assert.Throws<ArgumentException>(() => RiskTrendBuilder.Build(scoreA, scoreB));
    }

    [Fact]
    public void ComplianceReadinessBuilder_NoCompletedElements_AllDimensionsNotReady()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");

        ComplianceReadiness readiness = ComplianceReadinessBuilder.Build(facility);

        Assert.False(readiness.AuditReady);
        Assert.False(readiness.TrainingReady);
        Assert.False(readiness.PermitReady);
        Assert.False(readiness.SopReady);
        Assert.Equal(0d, readiness.OverallReadinessPercent);
    }

    [Fact]
    public void ComplianceReadinessBuilder_AllElementsCompleted_FullyReady()
    {
        FacilityProfile draft = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        var allCodes = draft.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet();
        FacilityProfile facility = FacilityProfileBuilder.Build(draft.FacilityId, "X", "Chemical Manufacturing", "Loc", allCodes);

        ComplianceReadiness readiness = ComplianceReadinessBuilder.Build(facility);

        Assert.True(readiness.AuditReady);
        Assert.True(readiness.TrainingReady);
        Assert.True(readiness.PermitReady);
        Assert.True(readiness.SopReady);
        Assert.Equal(100d, readiness.OverallReadinessPercent);
    }

    [Fact]
    public void RiskReadinessBuilder_AllElementsCompleted_FullyReady()
    {
        FacilityProfile draft = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        var allCodes = draft.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet();
        FacilityProfile facility = FacilityProfileBuilder.Build(draft.FacilityId, "X", "Chemical Manufacturing", "Loc", allCodes);

        RiskReadiness readiness = RiskReadinessBuilder.Build(facility);

        Assert.True(readiness.HazardResponseReady);
        Assert.True(readiness.EmergencyResponseReady);
        Assert.True(readiness.EventRoutingReady);
        Assert.Equal(100d, readiness.OverallReadinessPercent);
    }

    [Fact]
    public void ComplianceRiskEngine_ComputeReadiness_ReturnsBothComplianceAndRisk()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Medical Devices", "Loc");

        (ComplianceReadiness complianceReadiness, RiskReadiness riskReadiness) = ComplianceRiskEngine.ComputeReadiness(facility);

        Assert.Equal(facility.FacilityId, complianceReadiness.FacilityId);
        Assert.Equal(facility.FacilityId, riskReadiness.FacilityId);
    }

    [Fact]
    public void ComplianceRiskEngine_NeverThrows_AcrossAllIndustries()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), industry.Name, industry.Name, "Loc");

            ComplianceScore complianceScore = ComplianceRiskEngine.ComputeComplianceScore(facility);
            RiskScore riskScore = ComplianceRiskEngine.ComputeRiskScore(facility);
            ComplianceRiskEngine.ComputeComplianceDashboard(facility);
            ComplianceRiskEngine.ComputeRiskDashboard(facility);
            ComplianceRiskEngine.ComputeReadiness(facility);
            ComplianceRiskEngine.ComputeComplianceTrend(complianceScore, complianceScore);
            ComplianceRiskEngine.ComputeRiskTrend(riskScore, riskScore);
        }
    }

    [Fact]
    public void ComplianceDashboardMetadataBuilder_WithTrend_IncludesTrendKpis()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Medical Devices", "Loc");
        ComplianceScore score = ComplianceScoreBuilder.Build(facility);
        ComplianceTrend trend = ComplianceTrendBuilder.Build(score, score);

        ComplianceDashboardMetadata withoutTrend = ComplianceDashboardMetadataBuilder.Build(facility);
        ComplianceDashboardMetadata withTrend = ComplianceDashboardMetadataBuilder.Build(facility, trend);

        Assert.Null(withoutTrend.Trend);
        Assert.NotNull(withTrend.Trend);
        Assert.True(withTrend.KeyIndicators.Count > withoutTrend.KeyIndicators.Count);
    }

    [Fact]
    public void RiskDashboardMetadataBuilder_RequiredAlerts_OnlyForHighOrCriticalDimensions()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        RiskScore score = RiskScoreBuilder.Build(facility);
        RiskDashboardMetadata dashboard = RiskDashboardMetadataBuilder.Build(facility);

        int expectedAlertCount = new[] { score.HazardRiskLevel, score.ProgramRiskLevel, score.RegulatorRiskLevel, score.EventRiskLevel }
            .Count(level => level is RiskLevel.High or RiskLevel.Critical);

        Assert.Equal(expectedAlertCount, dashboard.RequiredAlerts.Count);
    }
}
