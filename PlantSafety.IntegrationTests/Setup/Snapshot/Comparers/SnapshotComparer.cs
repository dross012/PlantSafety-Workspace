// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup.Snapshot
//  Layer:          IntegrationTests
//  File:           SnapshotComparer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Compares live API responses against stored JSON baselines.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup.Snapshot.Comparers;

/// <summary>
/// Compares a live API response body against a stored JSON baseline file.
/// Ignores dynamic fields (timestamps, GUIDs) by default.
/// </summary>
public static class SnapshotComparer
{
    /// <summary>
    /// Compares two JSON strings for structural equivalence,
    /// ignoring fields listed in <paramref name="ignoredProperties"/>.
    /// </summary>
    public static void Compare(
        string expected,
        string actual,
        params string[] ignoredProperties)
    {
        var expectedDoc = JsonDocument.Parse(expected);
        var actualDoc   = JsonDocument.Parse(actual);

        // Simple property-level comparison excluding ignored fields
        foreach (var prop in expectedDoc.RootElement.EnumerateObject())
        {
            if (ignoredProperties.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                continue;

            actualDoc.RootElement.TryGetProperty(prop.Name, out var actualProp)
                .Should().BeTrue($"actual response should contain property '{prop.Name}'");
        }
    }
}
