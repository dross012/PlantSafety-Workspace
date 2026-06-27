// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup.Snapshot
//  Layer:          IntegrationTests
//  File:           BaselineReadme.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Documents the snapshot baseline policy for integration test result comparison.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup.Snapshot.Baselines;

/// <summary>
/// Documents the snapshot baseline policy.
/// Baseline JSON files are stored in this folder and compared against live API responses
/// to detect unintended contract changes.  Update baselines ONLY after intentional changes.
/// </summary>
public static class BaselineReadme
{
    /// <summary>Relative path where baseline JSON files are stored.</summary>
    public const string BaselineDirectory = "Setup/Snapshot/Baselines";
}
