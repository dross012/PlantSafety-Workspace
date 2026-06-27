// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TempDir.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Provides a deterministic, isolated temporary directory per test run.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Creates and owns a temporary directory for tests that require file I/O.
/// Cleaned up automatically when the test completes via <see cref="IDisposable"/>.
/// </summary>
public sealed class TempDir : IDisposable
{
    /// <summary>Full path to the temporary directory.</summary>
    public string Path { get; } =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary>Creates the directory on construction.</summary>
    public TempDir() => Directory.CreateDirectory(Path);

    /// <summary>Deletes the directory and all contents on disposal.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
