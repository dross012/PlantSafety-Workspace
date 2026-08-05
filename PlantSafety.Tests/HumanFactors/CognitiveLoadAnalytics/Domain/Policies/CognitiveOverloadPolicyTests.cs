// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Policies
//  Layer:          Tests (Unit)
//  File:           CognitiveOverloadPolicyTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Threshold-boundary tests for CognitiveOverloadPolicy.Classify/IsOverload.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Policies;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Policies;

public sealed class CognitiveOverloadPolicyTests
{
    [Theory]
    [InlineData(0, CognitiveWorkloadLevel.Low)]
    [InlineData(34.99, CognitiveWorkloadLevel.Low)]
    [InlineData(35, CognitiveWorkloadLevel.Moderate)]
    [InlineData(59.99, CognitiveWorkloadLevel.Moderate)]
    [InlineData(60, CognitiveWorkloadLevel.High)]
    [InlineData(79.99, CognitiveWorkloadLevel.High)]
    [InlineData(80, CognitiveWorkloadLevel.Overload)]
    [InlineData(100, CognitiveWorkloadLevel.Overload)]
    public void Classify_AtBoundaries_ReturnsExpectedLevel(decimal score, CognitiveWorkloadLevel expected)
    {
        CognitiveOverloadPolicy.Classify(score).Should().Be(expected);
    }

    [Theory]
    [InlineData(79.99, false)]
    [InlineData(80, true)]
    [InlineData(100, true)]
    public void IsOverload_AtBoundary_ReturnsExpected(decimal score, bool expected)
    {
        CognitiveOverloadPolicy.IsOverload(score).Should().Be(expected);
    }
}
