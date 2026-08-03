// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelVersioningTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Mechanical, reflection-based backward-compatibility test for the enterprise
//                  canonical model (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel).
//                  Asserts every V(n) public property (name + declared type) still exists on
//                  V(n+1) for each of the six canonical concepts -- a real, executable enforcement
//                  of the additive-only rule documented in
//                  PlantSafety.SharedKernel/Contracts/Enterprise/CanonicalModel/README.md, not just
//                  documentation. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Reflection;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Reflection-based backward-compatibility tests for every canonical Vn/Vn+1 type pair.</summary>
public sealed class CanonicalModelVersioningTests : UnitTestBase
{
    /// <summary>Every (V1, V2, V3) type triple for the six canonical concepts, keyed by concept name.</summary>
    public static TheoryData<string, Type, Type, Type> CanonicalTypeTriples => new()
    {
        { nameof(CanonicalAssetV1), typeof(CanonicalAssetV1), typeof(CanonicalAssetV2), typeof(CanonicalAssetV3) },
        { nameof(CanonicalWorkOrderV1), typeof(CanonicalWorkOrderV1), typeof(CanonicalWorkOrderV2), typeof(CanonicalWorkOrderV3) },
        { nameof(CanonicalProductionOrderV1), typeof(CanonicalProductionOrderV1), typeof(CanonicalProductionOrderV2), typeof(CanonicalProductionOrderV3) },
        { nameof(CanonicalRiskEventV1), typeof(CanonicalRiskEventV1), typeof(CanonicalRiskEventV2), typeof(CanonicalRiskEventV3) },
        { nameof(CanonicalImprovementActionV1), typeof(CanonicalImprovementActionV1), typeof(CanonicalImprovementActionV2), typeof(CanonicalImprovementActionV3) },
        { nameof(CanonicalSchedulingBlockV1), typeof(CanonicalSchedulingBlockV1), typeof(CanonicalSchedulingBlockV2), typeof(CanonicalSchedulingBlockV3) },
    };

    [Theory]
    [MemberData(nameof(CanonicalTypeTriples))]
    public void V2_ShouldBeAdditiveOnlySuperset_OfV1(string conceptName, Type v1, Type v2, Type v3)
    {
        _ = conceptName;
        _ = v3;
        AssertAdditiveOnlySuperset(v1, v2);
    }

    [Theory]
    [MemberData(nameof(CanonicalTypeTriples))]
    public void V3_ShouldBeAdditiveOnlySuperset_OfV2(string conceptName, Type v1, Type v2, Type v3)
    {
        _ = conceptName;
        _ = v1;
        AssertAdditiveOnlySuperset(v2, v3);
    }

    [Theory]
    [MemberData(nameof(CanonicalTypeTriples))]
    public void EveryCanonicalType_ShouldExposeMatchingSchemaVersionConstant(string conceptName, Type v1, Type v2, Type v3)
    {
        _ = conceptName;
        GetSchemaVersionConstant(v1).Should().Be(1, because: $"{v1.Name}.SchemaVersion must equal 1");
        GetSchemaVersionConstant(v2).Should().Be(2, because: $"{v2.Name}.SchemaVersion must equal 2");
        GetSchemaVersionConstant(v3).Should().Be(3, because: $"{v3.Name}.SchemaVersion must equal 3");
    }

    /// <summary>
    /// Asserts every public instance property (by name and declared type) on <paramref name="olderVersion"/>
    /// also exists, unchanged, on <paramref name="newerVersion"/>. This is the mechanical
    /// enforcement of "Vn+1 only ADDS to Vn" -- removal, rename, or retype of any Vn property fails
    /// this test.
    /// </summary>
    private static void AssertAdditiveOnlySuperset(Type olderVersion, Type newerVersion)
    {
        var olderProperties = GetPublicInstanceProperties(olderVersion);
        var newerProperties = GetPublicInstanceProperties(newerVersion);

        foreach (var (name, type) in olderProperties)
        {
            newerProperties.Should().ContainKey(name,
                because: $"{newerVersion.Name} must not remove {olderVersion.Name}.{name}");
            newerProperties[name].Should().Be(type,
                because: $"{newerVersion.Name}.{name} must not change type relative to {olderVersion.Name}.{name}");
        }
    }

    private static Dictionary<string, Type> GetPublicInstanceProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p.PropertyType);

    private static int GetSchemaVersionConstant(Type type)
    {
        var field = type.GetField("SchemaVersion", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"{type.Name} must declare a public const int SchemaVersion.");
        return (int)field.GetRawConstantValue()!;
    }
}
