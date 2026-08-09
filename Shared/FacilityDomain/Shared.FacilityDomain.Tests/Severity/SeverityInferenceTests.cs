// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           SeverityInferenceTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.3 unit tests for SeverityInferenceRules and
//                  SeverityMetadata: deterministic classification from
//                  hazard type, injury count, failure count, and event
//                  chain length, and the worst-case combination rule.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Xunit;

namespace Shared.FacilityDomain.Tests.Severity;

public sealed class SeverityInferenceTests
{
    [Theory]
    [InlineData(HazardType.Fire, SeverityLevel.Severe)]
    [InlineData(HazardType.ChemicalSpill, SeverityLevel.Severe)]
    [InlineData(HazardType.EnvironmentalRelease, SeverityLevel.Moderate)]
    [InlineData(HazardType.Other, SeverityLevel.Minor)]
    public void InferFromHazardType_ReturnsExpectedBaseline(HazardType hazardType, SeverityLevel expected)
    {
        SeverityInferenceRules.InferFromHazardType(hazardType).Should().Be(expected);
    }

    [Theory]
    [InlineData(0, SeverityLevel.Minor)]
    [InlineData(1, SeverityLevel.Moderate)]
    [InlineData(2, SeverityLevel.Severe)]
    [InlineData(5, SeverityLevel.Catastrophic)]
    public void InferFromInjuryCount_ReturnsExpectedLevel(int injuryCount, SeverityLevel expected)
    {
        SeverityInferenceRules.InferFromInjuryCount(injuryCount).Should().Be(expected);
    }

    [Theory]
    [InlineData(0, SeverityLevel.Minor)]
    [InlineData(1, SeverityLevel.Moderate)]
    [InlineData(2, SeverityLevel.Severe)]
    [InlineData(4, SeverityLevel.Catastrophic)]
    public void InferFromMachineFailureCount_ReturnsExpectedLevel(int failureCount, SeverityLevel expected)
    {
        SeverityInferenceRules.InferFromMachineFailureCount(failureCount).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, SeverityLevel.Minor)]
    [InlineData(2, SeverityLevel.Moderate)]
    [InlineData(3, SeverityLevel.Severe)]
    [InlineData(6, SeverityLevel.Catastrophic)]
    public void InferFromEventChainLength_ReturnsExpectedLevel(int chainLength, SeverityLevel expected)
    {
        SeverityInferenceRules.InferFromEventChainLength(chainLength).Should().Be(expected);
    }

    [Fact]
    public void Infer_TakesWorstCaseAcrossAllFactors()
    {
        SeverityLevel result = SeverityInferenceRules.Infer(HazardType.Other, employeeInjuryCount: 3, machineFailureCount: 0, eventChainLength: 1);

        result.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Infer_AllMinimalFactors_ReturnsMinor()
    {
        SeverityLevel result = SeverityInferenceRules.Infer(HazardType.Other, employeeInjuryCount: 0, machineFailureCount: 0, eventChainLength: 1);

        result.Should().Be(SeverityLevel.Minor);
    }

    [Theory]
    [InlineData(SeverityLevel.Minor, 1)]
    [InlineData(SeverityLevel.Moderate, 2)]
    [InlineData(SeverityLevel.Severe, 3)]
    [InlineData(SeverityLevel.Catastrophic, 4)]
    public void SeverityMetadata_WeightOf_ReturnsAscendingWeight(SeverityLevel level, int expectedWeight)
    {
        SeverityMetadata.WeightOf(level).Should().Be(expectedWeight);
    }

    [Fact]
    public void SeverityMetadata_DescriptionOf_ReturnsNonEmptyDescriptionForEveryLevel()
    {
        foreach (SeverityLevel level in Enum.GetValues<SeverityLevel>())
        {
            SeverityMetadata.DescriptionOf(level).Should().NotBeNullOrWhiteSpace();
        }
    }
}
