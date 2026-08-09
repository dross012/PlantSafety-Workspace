// ============================================================================
//  Project:        Shared.SnapshotTesting
//  File:           SnapshotHarness.cs
//  Created:        2026-08-09
//  Description:    Phase 2.10 deliverable: the snapshot test harness +
//                  baseline enforcement engine. VerifyAsync canonicalizes
//                  the actual JSON (stable indentation/property order via
//                  round-tripping through JsonDocument) and compares it
//                  against a golden file. BaselineMode (env var
//                  SNAPSHOT_BASELINE=true) writes/overwrites the golden
//                  file instead of comparing -- the deliberate,
//                  explicit-opt-in way to accept a real, reviewed output
//                  change as the new baseline, never silently.
//                  Golden files are meant to be committed to source
//                  control (the "golden-file repository" deliverable) so
//                  a diff in one is a reviewable part of the change that
//                  caused it.
// ============================================================================
using System.Text.Json;

namespace Shared.SnapshotTesting;

/// <summary>The snapshot test harness: compares actual JSON output against a committed golden file.</summary>
public static class SnapshotHarness
{
    private const string BaselineModeEnvironmentVariable = "SNAPSHOT_BASELINE";

    /// <summary>True if running in baseline mode (writes/overwrites golden files instead of comparing).</summary>
    public static bool BaselineMode =>
        string.Equals(Environment.GetEnvironmentVariable(BaselineModeEnvironmentVariable), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Verifies actualJson against the golden file "{snapshotDirectory}/{snapshotName}.json".
    /// In BaselineMode, writes/overwrites the golden file instead of comparing. Pass
    /// <paramref name="baselineModeOverride"/> to bypass the SNAPSHOT_BASELINE environment
    /// variable (used by Shared.SnapshotTesting's own tests, so they don't depend on
    /// process-global environment state).
    /// </summary>
    public static async Task VerifyAsync(string snapshotDirectory, string snapshotName, string actualJson, bool? baselineModeOverride = null)
    {
        string canonicalActual = Canonicalize(actualJson);
        string path = Path.Combine(snapshotDirectory, $"{snapshotName}.json");

        if (baselineModeOverride ?? BaselineMode)
        {
            Directory.CreateDirectory(snapshotDirectory);
            await File.WriteAllTextAsync(path, canonicalActual);
            return;
        }

        if (!File.Exists(path))
        {
            throw new SnapshotMismatchException(
                snapshotName,
                $"No golden file exists yet at {path}. Run with {BaselineModeEnvironmentVariable}=true to create one, review it, then commit it.",
                expected: string.Empty,
                actual: canonicalActual);
        }

        string canonicalExpected = Canonicalize(await File.ReadAllTextAsync(path));
        if (!string.Equals(canonicalExpected, canonicalActual, StringComparison.Ordinal))
        {
            throw new SnapshotMismatchException(snapshotName, "Actual output differs from the golden file.", canonicalExpected, canonicalActual);
        }
    }

    private static string Canonicalize(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
}
