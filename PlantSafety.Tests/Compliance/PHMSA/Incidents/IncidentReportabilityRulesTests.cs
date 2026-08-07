// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Incidents
//  Layer:          Tests (Unit)
//  File:           IncidentReportabilityRulesTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IncidentReportabilityRules.IsReportableToPhmsa, the
//                  illustrative 49 CFR 171.15/171.16 reportability computation.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Rules;

namespace PlantSafety.Tests.Compliance.PHMSA.Incidents;

public sealed class IncidentReportabilityRulesTests : UnitTestBase
{
    [Fact]
    public void IsReportableToPhmsa_WithFatalitiesInvolved_ReturnsTrue()
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.Injury, injuries: 0, fatalitiesInvolved: true, propertyDamageEstimateUsd: null);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsReportableToPhmsa_SpillWithNoInjuriesFatalitiesOrDamage_ReturnsFalse()
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.Spill, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsReportableToPhmsa_FireOrExplosion_AlwaysReturnsTrue()
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.FireOrExplosion, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsReportableToPhmsa_WithAnyInjuries_ReturnsTrue()
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.Injury, injuries: 1, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(50_000)]
    [InlineData(75_000)]
    public void IsReportableToPhmsa_PropertyDamageAtOrAboveIllustrativeThreshold_ReturnsTrue(decimal damage)
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.PropertyDamage, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: damage);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(49_999.99)]
    public void IsReportableToPhmsa_PropertyDamageBelowIllustrativeThreshold_ReturnsFalse(decimal damage)
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.PropertyDamage, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: damage);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsReportableToPhmsa_NoDamageProvided_DoesNotThrowAndReturnsFalseWhenNoOtherCriteriaMet()
    {
        var result = IncidentReportabilityRules.IsReportableToPhmsa(
            type: IncidentType.Leak, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        result.Should().BeFalse();
    }
}
